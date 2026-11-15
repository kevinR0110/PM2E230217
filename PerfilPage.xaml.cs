using System.Net;

namespace PM2E230217;

public partial class PerfilPage : ContentPage
{
    public PerfilPage()
    {
        InitializeComponent();

        // Sin flecha de regreso: para salir hay que usar "Cerrar sesión"
        NavigationPage.SetHasBackButton(this, false);
    }

    // Se ejecuta cada vez que la página aparece: aquí se pide el perfil a la API
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var perfil = await ApiService.ObtenerPerfilAsync();

            NombreLabel.Text = perfil.Nombre;
            DisponibleSwitch.IsToggled = perfil.Disponible;

            int anios = Math.Clamp(perfil.AniosExperiencia, 0, 15);
            ExperienciaSlider.Value = anios;
            ActualizarNivel(anios);   // se llama aquí por si el Slider ya valía ese número
        }
        catch (ApiException ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");

            // Token inválido o vencido: volver al Login
            if (ex.Codigo == HttpStatusCode.Unauthorized)
            {
                ApiService.CerrarSesion();
                await Navigation.PopAsync();
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Sin conexión", "No se pudo conectar con la API. Verifica que esté encendida.", "OK");
        }
    }

    // Se ejecuta mientras se mueve el Slider
    private void OnSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        ActualizarNivel((int)Math.Round(e.NewValue));
    }

    // Cambia el texto y el color del Border según los años (3 niveles)
    private void ActualizarNivel(int anios)
    {
        string nivel;
        Color fondo;

        if (anios <= 2)
        {
            nivel = "Nivel Inicial";
            fondo = Color.FromArgb("#DCFCE7");
        }
        else if (anios <= 7)
        {
            nivel = "Nivel Intermedio";
            fondo = Color.FromArgb("#E0F2FE");
        }
        else
        {
            nivel = "Nivel Avanzado";
            fondo = Color.FromArgb("#FEF3C7");
        }

        AniosLabel.Text = anios == 1 ? "1 año de experiencia" : $"{anios} años de experiencia";
        NivelLabel.Text = nivel;
        NivelBorder.BackgroundColor = fondo;
    }

    // Cerrar sesión: borra el token y regresa al Login
    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        ApiService.CerrarSesion();
        await Navigation.PopAsync();
    }
}