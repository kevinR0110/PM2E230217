using System.Net;

namespace PM2E230217;

public partial class LoginPage : ContentPage
{
    private const int MaxIntentos = 3;
    private int intentosFallidos = 0;   

    public LoginPage()
    {
        InitializeComponent();
    }


    private void OnMostrarClaveChanged(object? sender, CheckedChangedEventArgs e)
    {
        ClaveEntry.IsPassword = !e.Value;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string correo = CorreoEntry.Text?.Trim() ?? "";
        string clave = ClaveEntry.Text ?? "";

      
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            await DisplayAlert("Campos vacíos", "Escribe tu correo y tu contraseña.", "OK");
            return;
        }


        LoginBtn.IsEnabled = false;
        Cargando.IsVisible = true;
        Cargando.IsRunning = true;

        bool exito = false;

        try
        {
            await ApiService.LoginAsync(correo, clave);
            exito = true;
            intentosFallidos = 0;
            ActualizarIntentos();
        }
        catch (ApiException ex)
        {
            
            if (ex.Codigo == HttpStatusCode.Unauthorized)
            {
                intentosFallidos++;
                ActualizarIntentos();

                if (intentosFallidos >= MaxIntentos)
                {
                    await DisplayAlert("Botón bloqueado", "Fallaste 3 veces. El botón de iniciar sesión fue bloqueado.", "OK");
                }
                else
                {
                    await DisplayAlert("Acceso denegado", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        catch (Exception)
        {
            
            await DisplayAlert("Sin conexión", "No se pudo conectar con la API. Verifica que esté encendida.", "OK");
        }
        finally
        {
            
            Cargando.IsRunning = false;
            Cargando.IsVisible = false;
            LoginBtn.IsEnabled = intentosFallidos < MaxIntentos;
        }

       
        if (exito)
        {
            ClaveEntry.Text = "";
            await Navigation.PushAsync(new PerfilPage());
        }
    }

    private void ActualizarIntentos()
    {
        IntentosLabel.Text = $"Intentos fallidos: {intentosFallidos} de {MaxIntentos}";
    }
}