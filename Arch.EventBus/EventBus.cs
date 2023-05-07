using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.Bus;

/// <summary>
///     The EventBus model
/// </summary>
public struct EventBus
{
    /// <summary>
    ///     The namespace.
    /// </summary>
    public string Namespace { get; set; }
    
    /// <summary>
    ///     The <see cref="Method"/>s of the <see cref="EventBus"/> "redirecting" the event.
    /// </summary>
    public IList<Method> Methods;
}

/// <summary>
///     A method inside the eventbus redirecting the event towards the receivers. 
/// </summary>
public struct Method
{
    /// <summary>
    ///     The <see cref="RefKind"/> this method accepts as a param.
    /// </summary>
    public RefKind RefKind;
    
    /// <summary>
    ///     The Event type as a <see cref="ITypeSymbol"/> that is being passed to the method and being redirected.
    /// </summary>
    public ITypeSymbol EventType;
    
    /// <summary>
    ///     A list of methods which this <see cref="Method"/> redirects the event to.
    /// </summary>
    public IList<ReceivingMethod> EventReceivingMethods;
}

/// <summary>
///     The <see cref="ReceivingMethod"/> struct
///     represents a method that receives the event (and is marked by the event tag) with its order. 
/// </summary>
public struct ReceivingMethod
{

    /// <summary>
    ///     If the receiving method is static.
    ///     If not, we are targeting instances and need to handle them differently during generation. 
    /// </summary>
    public bool Static;
    
    /// <summary>
    ///     The method symbol of the static event receiver which should be called.
    /// </summary>
    public IMethodSymbol MethodSymbol;
    
    /// <summary>
    ///     Its order. 
    /// </summary>
    public int Order;
}

public static class EventBusExtensions
{
    
        
    /// <summary>
    ///     Convert a <see cref="RefKind"/> to its code string equivalent.
    /// </summary>
    /// <param name="refKind">The <see cref="RefKind"/>.</param>
    /// <returns>The code string equivalent.</returns>
    public static string RefKindToString(RefKind refKind)
    {
        switch (refKind)
        {
            case RefKind.None:
                return "";
            case RefKind.Ref:
                return "ref";
            case RefKind.In:
                return "in";
            case RefKind.Out:
                return "out";
        }
        return null;
    }

    /// <summary>
    ///     Appends all methods redirecting events.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="callMethods">The <see cref="IList{T}"/> containing the <see cref="Method"/>s redirecting the event and calling the methods. </param>
    /// <returns></returns>
    public static StringBuilder AppendEventMethods(this StringBuilder sb, IList<Method> callMethods)
    {
        foreach (var eventCallMethod in callMethods)
        {
            sb.AppendEventMethod(eventCallMethod);
        }
        return sb;
    }
    
    /// <summary>
    ///     Appends all methods redirecting events.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="callMethods">The <see cref="IList{T}"/> containing the <see cref="Method"/>s redirecting the event and calling the methods. </param>
    /// <returns></returns>
    public static StringBuilder AppendEventMethod(this StringBuilder sb, Method callMethod)
    {
        var callMethodsInOrder = new StringBuilder().MethodCalls(callMethod);
        var instanceReceiverLists = new StringBuilder().InstanceReceiverLists(callMethod);
        
        var template = $$"""
        {{instanceReceiverLists}}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Send({{RefKindToString(callMethod.RefKind)}} {{callMethod.EventType.ToDisplayString()}} {{callMethod.EventType.Name.ToLower()}}){
            {{callMethodsInOrder}}
        }
        """;
        sb.AppendLine(template);
        return sb;
    }
    
    
    /// <summary>
    ///     Appends calls to all event receiving method.
    ///     <remarks>SomeMethod(...); OtherMethod(...); ...</remarks>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="callMethod">The <see cref="Method"/> the event call which methods will be called after one another.</param>
    /// <returns></returns>
    public static StringBuilder MethodCalls(this StringBuilder sb, Method callMethod)
    {
        // Loop over every found method receiver
        foreach (var eventReceivingMethod in callMethod.EventReceivingMethods)
        {
            var containingSymbol = eventReceivingMethod.MethodSymbol.ContainingSymbol;
            var methodName = eventReceivingMethod.MethodSymbol.Name;
            var passEvent = $"{RefKindToString(callMethod.RefKind)} {callMethod.EventType.Name.ToLower()}";
            
            // Remove weird chars to also support value tuples flawlessly, otherwhise they are listed like (World world, int int) in code which destroys everything
            var eventType = callMethod.EventType.ToString();
            eventType = eventType.Replace("(","").Replace(")","").Replace(".","_").Replace(",","_").Replace(" ","");
            
            // If static, call directly... if non static, loop over the instances for this event and call them one by one.
            if (eventReceivingMethod.Static)
            {
                sb.AppendLine($"{containingSymbol}.{methodName}({passEvent});");   
            }
            else
            {
                var instanceList = $"{containingSymbol.Name}_{methodName}_{eventType}";
                var template = $$"""
                    for(var index = 0; index < {{instanceList}}.Count; index++)
                    {
                        {{instanceList}}[index].{{methodName}}({{passEvent}});
                    }
                """;
                sb.AppendLine(template);
            }
        }
        return sb;
    }
    
    /// <summary>
    ///     Appends lists for a certain <see cref="Method"/> containing one list for each instance (non static) receiving a method.
    ///     <remarks>List&lt;SomeInstance&gt; SomeInstance_OnSomeEvent_EventType; ...</remarks>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="callMethod">The <see cref="Method"/> the event call which methods will be called after one another.</param>
    /// <returns></returns>
    public static StringBuilder InstanceReceiverLists(this StringBuilder sb, Method callMethod)
    {
        foreach (var eventReceivingMethod in callMethod.EventReceivingMethods)
        {
            var containingSymbol = eventReceivingMethod.MethodSymbol.ContainingSymbol;
            var methodName = eventReceivingMethod.MethodSymbol.Name;
            
            // Remove weird chars to also support value tuples flawlessly, otherwhise they are listed like (World world, int int) in code which destroys everything
            var eventType = callMethod.EventType.ToString();
            eventType = eventType.Replace("(","").Replace(")","").Replace(".","_").Replace(",","_").Replace(" ","");
            
            if (eventReceivingMethod.Static)
            {
                continue;
            }
            
            sb.AppendLine($"public static List<{containingSymbol}> {containingSymbol.Name}_{methodName}_{eventType} = new List<{containingSymbol}>(128);");
        }
        return sb;
    }

    /// <summary>
    /// Appends a <see cref="EventBus"/> and generates it.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="eventBus">The <see cref="EventBus"/> itself, used to generate the EventBus in code.</param>
    /// <returns></returns>
    public static StringBuilder AppendEventBus(this StringBuilder sb, ref EventBus eventBus)
    {
        var callerMethods = new StringBuilder().AppendEventMethods(eventBus.Methods);
        var template = $$"""
        using System.Runtime.CompilerServices;
        using System.Collections.Generic;
        namespace {{eventBus.Namespace}}{
            public partial class EventBus{
                {{callerMethods}}
            }
        }
        """;
        return sb.Append(template);
    }
}