namespace Audiosurf2_Tools.Entities;

public class Consts
{
    public static string GetTokenHtml = @"
    <HTML>
        <HEAD>
            <script>
                window.onload = function() {
                    window.location.href = 'http://localhost:8888?' + document.location.hash.replace('#', '');
		            document.getElementById('code').innerHTML = document.location.hash.replace('#', '');
                }
            </script>
        </HEAD>
        
        <BODY>
            <p id='code'></p>
        </BODY>
    </HTML>";
    
    public static string CloseWindowHtml = @"
    <HTML>
        <HEAD>
            <script>
                window.onload = function() {
                    window.close();
                }
            </script>
        </HEAD>
        
        <BODY>
        </BODY>
    </HTML>";
}