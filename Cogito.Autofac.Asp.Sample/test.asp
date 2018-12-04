<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>

    <%
        Dim obj
        Set obj = ComponentContext.Resolve("Cogito.Autofac.Asp.Sample.Objects.ResolvableObject")
        Set obj = ComponentContext.ResolveOwned("Cogito.Autofac.Asp.Sample.Objects.ResolvableObject")
        Dim val
        Set val = obj.Value
    %>

    <br />

    I resolved a .NET object from Autofac. The value is <%= val.Text %>.

</body>
</html>
