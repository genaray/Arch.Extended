using System.Diagnostics.CodeAnalysis;
using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Tests queries using data parameters.
/// </summary>
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Allow unused params for query headers")]
internal partial class DataParamSystem : BaseTestSystem
{
    public DataParamSystem(World world) : base(world) { }

    [Query]
    [All(typeof(IntComponentA))]
    public static void CountANoParams([Data] ref int count)
    {
        count++;
    }

    [Query]
    public static void CountAWithParamsLeft([Data] ref int count, in IntComponentA _)
    {
        count++;
    }

    [Query]
    public static void CountAWithParamsRight(in IntComponentA _, [Data] ref int count)
    {
        count++;
    }

    [Query]
    public static void CountAWithParamsMiddle(in IntComponentA _, [Data] ref int count, in IntComponentB __)
    {
        count++;
    }

    [Query]
    public static void CountATwiceWithParams([Data] ref int count1, in IntComponentA _, [Data] ref int count2, in IntComponentB __)
    {
        count1++;
        count2++;
    }

    [Query]
    [All(typeof(IntComponentA))]
    public static void CountAWithEntityRight(in Entity e, [Data] ref int count)
    {
        count++;
    }

    [Query]
    [All(typeof(IntComponentA))]
    public static void CountAWithEntityLeft([Data] ref int count, in Entity e)
    {
        count++;
    }

    [Query]
    public static void CountAWithEntityAndParamLeft([Data] ref int count, in IntComponentA a, in Entity e)
    {
        count++;
    }

    [Query]
    public static void CountAWithEntityAndParamRight(in Entity e, in IntComponentA a, [Data] ref int count)
    {
        count++;
    }

    // compilation fails from https://github.com/genaray/Arch.Extended/issues/89
    //[Query]
    //public void AssignEntityDataParamRight(in IntComponentA a, [Data] ref Entity outEntity)
    //{
    //    outEntity = _sampleEntity;
    //}

    // compilation fails from https://github.com/genaray/Arch.Extended/issues/89
    //[Query]
    //public void AssignEntityDataParamLeft([Data] ref Entity outEntity, in IntComponentA a)
    //{
    //    outEntity = _sampleEntity;
    //}

    [Query]
    public static void AssignEntityDataParamWithEntityRight(in Entity e, in IntComponentA a, [Data] ref Entity outEntity)
    {
        outEntity = e;
    }

    // compilation fails from https://github.com/genaray/Arch.Extended/issues/89
    //[Query]
    //public static void AssignEntityDataParamWithEntityLeft([Data] ref Entity outEntity, in Entity e, in IntComponentA a)
    //{
    //    outEntity = e;
    //}

    // Crashes source generator due to ? in filename; see https://github.com/genaray/Arch.Extended/issues/91
    //[Query]
    //[All(typeof(IntComponentA))]
    //public static void CountANullable([Data] ref int? count)
    //{
    //    count ??= 0;
    //    count++;
    //}

    private Entity _sampleEntity;
    public override void Setup()
    {
        _sampleEntity = World.Create(new IntComponentA(), new IntComponentB());
        World.Create(new IntComponentA(), new IntComponentB());
    }

    public override void Update(in int t)
    {
        int i = 0;
        CountANoParamsQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        CountAWithParamsLeftQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        CountAWithParamsRightQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        CountAWithParamsMiddleQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        int i2 = 0;
        CountATwiceWithParamsQuery(World, ref i, ref i2);
        Assert.Multiple(() =>
        {
            Assert.That(i, Is.EqualTo(2));
            Assert.That(i2, Is.EqualTo(2));
        });

        i = 0;
        CountAWithEntityRightQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        CountAWithEntityLeftQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        CountAWithEntityAndParamLeftQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        i = 0;
        CountAWithEntityAndParamRightQuery(World, ref i);
        Assert.That(i, Is.EqualTo(2));

        Entity outEntity = Entity.Null;
        //AssignEntityDataParamRightQuery(World, ref outEntity);
        //Assert.That(outEntity, Is.EqualTo(_sampleEntity));

        //outEntity = Entity.Null;
        //AssignEntityDataParamLeftQuery(World, ref outEntity);
        //Assert.That(outEntity, Is.EqualTo(_sampleEntity));

        outEntity = Entity.Null;
        AssignEntityDataParamWithEntityRightQuery(World, ref outEntity);
        Assert.That(outEntity, Is.Not.EqualTo(Entity.Null));

        //outEntity = Entity.Null;
        //AssignEntityDataParamWithEntityLeftQuery(World, ref outEntity);
        //Assert.That(outEntity, Is.Not.EqualTo(Entity.Null));

        //int? i3 = null;
        //CountANullableQuery(World, ref i3);
        //Assert.That(i, Is.EqualTo(2));
    }
}
