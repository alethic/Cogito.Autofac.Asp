<html>
<head>
    <title>Server Variables</title>
</head>
<body>
    <table>
        <tr>
            <td>
                <b>Server Varriable</b>
            </td>
            <td>
                <b>Value</b>
            </td>
        </tr>

        <% For Each name In Request.ServerVariables %>
        <tr>
            <td>
                <%= name %>
            </td>
            <td>
                <%= Request.ServerVariables(name) %>
            </td>
        </tr>
        <% Next %>

    </table>
</body>
</html>
