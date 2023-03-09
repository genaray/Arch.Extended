using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.EventBus.SourceGenerator;

/// <summary>
/// The EventBus model
/// </summary>
public struct EventBus
{
    /// <summary>
    /// The namespace.
    /// </summary>
    public string Namespace { get; set; }
    
    /// <summary>
    /// The <see cref="Method"/>s of the <see cref="EventBus"/> "redirecting" the event.
    /// </summary>
    public IList<Method> Methods;
}

/// <summary>
/// A method inside the eventbus redirecting the event towards the receivers. 
/// </summary>
public struct Method
{
    /// <summary>
    /// The <see cref="RefKind"/> this method accepts as a param.
    /// </summary>
    public RefKind RefKind;
    
    /// <summary>
    /// The Event type as a <see cref="ITypeSymbol"/> that is being passed to the method and being redirected.
    /// </summary>
    public ITypeSymbol EventType;
    
    /// <summary>
    /// A list of methods which this <see cref="Method"/> redirects the event to.
    /// </summary>
    public IList<IMethodSymbol> EventReceivingMethods;
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
    ///     Appends calls to all event receiving method.
    ///     <remarks>SomeMethod(...); OtherMethod(...); ...</remarks>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="callMethod">The <see cref="Method"/> the event call which methods will be called after one another.</param>
    /// <returns></returns>
    public static StringBuilder AppendEventMethod(this StringBuilder sb, Method callMethod)
    {
        foreach (var eventReceivingMethod in callMethod.EventReceivingMethods){
            sb.AppendLine($"{eventReceivingMethod.ContainingSymbol}.{eventReceivingMethod.Name}({RefKindToString(callMethod.RefKind)} {callMethod.EventType.Name.ToLower()});");
        }
        return sb;
    }
    
    /// <summary>
    ///     Appends all methods redirecting events.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="callMethods">The <see cref="IList{T}"/> containing the <see cref="Method"/>s redirecting the event and calling the methods. </param>
    /// <returns></returns>
    public static StringBuilder AppendEventMethod(this StringBuilder sb, IList<Method> callMethods)
    {
        foreach (var eventCallMethod in callMethods)
        {
            var callMethodsInOrder = new StringBuilder().AppendEventMethod(eventCallMethod);
            var template = $$"""
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Send({{RefKindToString(eventCallMethod.RefKind)}} {{eventCallMethod.EventType.ToDisplayString()}} {{eventCallMethod.EventType.Name.ToLower()}}){
                {{callMethodsInOrder}}
            }
            """;
            sb.AppendLine(template);
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
        var callerMethods = new StringBuilder().AppendEventMethod(eventBus.Methods);
        var template = $$"""
        using System.Runtime.CompilerServices;
        namespace {{eventBus.Namespace}};
        public partial class EventBus{
            {{callerMethods}}
        }
        """;
        return sb.Append(template);
    }
}