using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

/// <summary>
/// Represents the BaseSystem that is generated and calls its generated query methods.  
/// </summary>
public struct BaseSystem
{
    /// <summary>
    /// The namespace its generic is in.
    /// </summary>
    public string GenericTypeNamespace { get; set; }
    
    /// <summary>
    /// The namespace this system is in. 
    /// </summary>
    public string Namespace { get; set; }
    
    /// <summary>
    /// Its name.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The generic type. 
    /// </summary>
    public ITypeSymbol GenericType { get; set; }
    
    /// <summary>
    /// The Query methods this base system calls one after another. 
    /// </summary>
    public IList<IMethodSymbol> QueryMethods { get; set; }
}

/// <summary>
/// Represents the Query method that is generated. 
/// </summary>
public struct QueryMethod
{
    /// <summary>
    /// If the class containing this Query method is within the global namespace.
    /// </summary>
    public bool IsGlobalNamespace { get; set; }
    
    /// <summary>
    /// The namespace of the method.
    /// </summary>
    public string Namespace { get; set; }
    
    /// <summary>
    /// If this method is static.
    /// </summary>
    public bool IsStatic { get; set; }
    
    /// <summary>
    /// If this Query method contains an Entity as a param and acesses it. 
    /// </summary>
    public bool IsEntityQuery { get; set; }
    
    /// <summary>
    /// The name of the class containing this Query method.
    /// </summary>
    public string ClassName { get; set; }
    
    /// <summary>
    /// The name of the Query method.
    /// </summary>
    public string MethodName { get; set; }
    
    /// <summary>
    /// The entity parameter, if its an entity query. 
    /// </summary>
    public IParameterSymbol EntityParameter { get; set; }
    
    /// <summary>
    /// All parameters within the query method, not only the components. Also Entity and Data annotated ones.
    /// <remarks>public void Query([Data] float time, in Entity entity, ...);</remarks>
    /// </summary>
    public IList<IParameterSymbol> Parameters { get; set; }

    /// <summary>
    /// The Components acessed within the query method.
    /// <remarks>public void Query(ref Position pos, in Velocity vel){}</remarks>
    /// </summary>
    public IList<IParameterSymbol> Components { get; set; }
    
    /// <summary>
    /// All <see cref="ITypeSymbol"/>s mentioned in the All annotation query filter.
    /// <remarks>[All(typeof(Position), typeof(Velocity)] or its generic variant</remarks>
    /// </summary>
    public IList<ITypeSymbol> AllFilteredTypes { get; set; }
    
    /// <summary>
    /// All <see cref="ITypeSymbol"/>s mentioned in the Any annotation query filter.
    /// <remarks>[Any(typeof(Position), typeof(Velocity)] or its generic variant</remarks>
    /// </summary>
    public IList<ITypeSymbol> AnyFilteredTypes { get; set; }
    
    /// <summary>
    /// All <see cref="ITypeSymbol"/>s mentioned in the None annotation query filter.
    /// <remarks>[None(typeof(Position), typeof(Velocity)] or its generic variant</remarks>
    /// </summary>
    public IList<ITypeSymbol> NoneFilteredTypes { get; set; }
    
    /// <summary>
    /// All <see cref="ITypeSymbol"/>s mentioned in the Exclusive annotation query filter.
    /// <remarks>[Exclusive(typeof(Position), typeof(Velocity)] or its generic variant</remarks>
    /// </summary>
    public IList<ITypeSymbol> ExclusiveFilteredTypes { get; set; }
}