using System;
class Program
{
    static void FakeMain()
    {
        SomeMethod();
        var foo = new Foo();
        foo.OtherMethod();
        int a;
        a = 3 + foo.AnotherMethod();
    }

    static void SomeMethod()
    {
        var bar = new Bar();
        bar.MethodB();
    }
}

class Bar
{
    public void MethodA()
    {
    }

    public void MethodB()
    {
        Parent one = new Child();
        one.Method1();
    }
}
class Foo
{
    public void OtherMethod()
    {
    }

    public int AnotherMethod()
    {
        return 0;
    }
}

class Parent
{
    public virtual void Method1()
    {
    }
}

class Child : Parent
{
    public override void Method1()
    {
    }
}
