using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PM2E230217
{
    // Error propio: guarda el mensaje que mandó la API y el código HTTP (401, 400...)
    public class ApiException : Exception
    {
        public HttpStatusCode Codigo { get; }

        public ApiException(string mensaje, HttpStatusCode codigo) : base(mensaje)
        {
            Codigo = codigo;
        }
    }

    // Datos del perfil que devuelve la API
    public class PerfilModel
    {
        public string Nombre { get; set; } = "";
        public bool Disponible { get; set; }
        public int AniosExperiencia { get; set; }
    }

    // static: una sola copia compartida por todas las pantallas
    public static class ApiService
    {
        // Rutas de la API. Si en tu API son otras (por ejemplo api/auth/login), cámbialas SOLO aquí.
        private const string RutaLogin = "api/usuarios/login";
        private const string RutaPerfil = "api/usuarios/perfil";

        // HttpClient: el "cartero" que lleva y trae mensajes de la API
        private static readonly HttpClient Http = new()
        {
            // Android (emulador) ve la PC como 10.0.2.2; Windows usa localhost
            BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5080/"
                : "http://localhost:5080/"),
            Timeout = TimeSpan.FromSeconds(10)   // si la API está apagada no esperamos para siempre
        };

        // Token que entrega la API al iniciar sesión. null = nadie ha entrado
        private static string? Token { get; set; }

        // POST al login: si los datos son correctos guarda el token
        public static async Task LoginAsync(string correo, string clave)
        {
            var resp = await Http.PostAsJsonAsync(RutaLogin, new { correo, clave });
            await RevisarAsync(resp);

            var datos = await resp.Content.ReadFromJsonAsync<TokenRespuesta>();
            if (datos == null || string.IsNullOrEmpty(datos.Token))
            {
                throw new ApiException("La API no devolvió un token.", HttpStatusCode.InternalServerError);
            }

            Token = datos.Token;

            // Desde ahora todas las peticiones llevan el token como Bearer
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        // GET del perfil: nombre, disponible y años de experiencia
        public static async Task<PerfilModel> ObtenerPerfilAsync()
        {
            var resp = await Http.GetAsync(RutaPerfil);
            await RevisarAsync(resp);

            var perfil = await resp.Content.ReadFromJsonAsync<PerfilModel>();
            return perfil ?? new PerfilModel();
        }

        // Borra el token: es cerrar sesión
        public static void CerrarSesion()
        {
            Token = null;
            Http.DefaultRequestHeaders.Authorization = null;
        }

        // Si la API respondió con error, lanza ApiException con el mensaje que mandó la API
        private static async Task RevisarAsync(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                return;
            }

            string mensaje = resp.StatusCode == HttpStatusCode.Unauthorized
                ? "Sesión no válida o credenciales incorrectas."
                : "La API respondió con un error.";

            try
            {
                // La API manda {"mensaje": "..."} cuando hay error
                var error = await resp.Content.ReadFromJsonAsync<ErrorRespuesta>();
                if (!string.IsNullOrWhiteSpace(error?.Mensaje))
                {
                    mensaje = error.Mensaje;
                }
            }
            catch
            {
                // Si la respuesta no es JSON, se deja el mensaje por defecto
            }

            throw new ApiException(mensaje, resp.StatusCode);
        }

        // Moldes para leer el JSON de la API
        private class TokenRespuesta
        {
            public string Token { get; set; } = "";
        }

        private class ErrorRespuesta
        {
            public string? Mensaje { get; set; }
        }
    }
}