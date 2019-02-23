<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>

    <%
        Dim obj
        Set obj = ComponentContext.Resolve("Cogito.Autofac.Asp.Sample.Objects.ResolvableObject, Cogito.Autofac.Asp.Sample.Objects")
    %>

    <br />

    I resolved a .NET object from Autofac. It should have an OWIN context available. The host value from it is '<%= obj.Host %>'.

</body>
</html>
