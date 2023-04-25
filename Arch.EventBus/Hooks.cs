using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.Bus;

public struct Hooks
{
    public List<ClassHooks> Instances;
}

public struct ClassHooks
{
    public ITypeSymbol PartialClass;
    public IList<EventHook> EventHooks;
}

/// <summary>
///     The <see cref="EventHook"/> struct
///     represents a hook for an event from the eventbus inside an class instance.
/// </summary>
public struct EventHook
{

    /// <summary>
    ///     The event type.
    /// </summary>
    public ITypeSymbol EventType;
    
    /// <summary>
    ///     The method symbol of the static event receiver which should be called.
    /// </summary>
    public IMethodSymbol MethodSymbol;
}

public static class Hookersxtensions
{
    
    /// <summary>
    ///     Appends add operations for a set of <see cref="EventHook"/> to hook the local class instance into the EventBus instance lists for receiving events. 
    ///     <remarks>EventBus.SomeClass_SomeEvent_SomeEvent.Add(this); ...</remarks>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="receivingMethods">The <see cref="List{T}"/> of <see cref="ReceivingMethod"/> that will be hooked in to receive instance events. </param>
    /// <returns></returns>
    public static StringBuilder Hook(this StringBuilder sb, IList<EventHook> receivingMethods)
    {
        foreach (var eventReceivingMethod in receivingMethods)
        {
            var containingSymbol = eventReceivingMethod.MethodSymbol.ContainingSymbol;
            var methodName = eventReceivingMethod.MethodSymbol.Name;
            
            // Remove weird chars to also support value tuples flawlessly, otherwhise they are listed like (World world, int int) in code which destroys everything
            var eventType = eventReceivingMethod.EventType.ToString();
            eventType = eventType.Replace("(","").Replace(")","").Replace(".","_").Replace(",","_").Replace(" ","");

            sb.AppendLine($"EventBus.{containingSymbol.Name}_{methodName}_{eventType}.Add(this);");
        }
        return sb;
    }
    
    /// <summary>
    ///     Appends remove operations for a set of <see cref="EventHook"/> to unhook the local class instance from the EventBus instance lists for receiving events. 
    ///     <remarks>EventBus.SomeClass_SomeEvent_SomeEvent.Remove(this); ...</remarks>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="receivingMethods">The <see cref="List{T}"/> of <see cref="ReceivingMethod"/> that will be hooked in to receive instance events. </param>
    /// <returns></returns>
    public static StringBuilder Unhook(this StringBuilder sb, IList<EventHook> receivingMethods)
    {
        foreach (var eventReceivingMethod in receivingMethods)
        {
            var containingSymbol = eventReceivingMethod.MethodSymbol.ContainingSymbol;
            var methodName = eventReceivingMethod.MethodSymbol.Name;
            
            // Remove weird chars to also support value tuples flawlessly, otherwhise they are listed like (World world, int int) in code which destroys everything
            var eventType = eventReceivingMethod.EventType.ToString();
            eventType = eventType.Replace("(","").Replace(")","").Replace(".","_").Replace(",","_").Replace(" ","");
            
            sb.AppendLine($"EventBus.{containingSymbol.Name}_{methodName}_{eventType}.Remove(this);");
        }
        return sb;
    }
    
    /// <summary>
    ///     Appends a <see cref="List{T}"/> of <see cref="Hook"/> and generates proper hook and unhook methods for their partial class instances.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="hooks">The <see cref="List{T}"/> of <see cref="Hook"/> itself, used to generate the hooks in code.</param>
    /// <returns></returns>
    public static StringBuilder AppendHookList(this StringBuilder sb, List<ClassHooks> hooks)
    {
        // Loop over all hooks to create the hook and unhook functions.
        foreach (var hook in hooks)
        {

            var hookIntoEventbus = new StringBuilder().Hook(hook.EventHooks);
            var unhookFromEventBus = new StringBuilder().Unhook(hook.EventHooks);
            
            var template = $$"""
            namespace {{hook.PartialClass.ContainingNamespace}}{

                public partial class {{hook.PartialClass.Name}}{
                    
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public void Hook()
                    {
                        {{hookIntoEventbus}}
                    }
                    
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public void Unhook()
                    {
                        {{unhookFromEventBus}}   
                    }
                }
            }
            """;
            sb.AppendLine(template);
        }

        return sb;
    }
    
    /// <summary>
    ///     Appends a <see cref="Hooks"/> and generates it.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="hooks">The <see cref="Hooks"/> itself, used to generate the hooks in code.</param>
    /// <returns></returns>
    public static StringBuilder AppendHooks(this StringBuilder sb, ref Hooks hooks)
    {
        var callerMethods = new StringBuilder().AppendHookList(hooks.Instances);
        var template = $$"""
        using System.Runtime.CompilerServices;
        using Arch.Bus;
        {{callerMethods}}
        """;
        return sb.Append(template);
    }
}