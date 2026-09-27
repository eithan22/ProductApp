namespace ProductApp.Domian.Common.Legal
{
    // Versión vigente de los Términos de Servicio y la Política de Privacidad.
    //
    // Vive acá y no en ConfiguracionSistema a propósito: el TEXTO de los documentos está en
    // las vistas del proyecto Web y se despliega junto con la aplicación, así que la versión
    // es una propiedad del código desplegado, no un parámetro que el administrador de un
    // negocio pueda editar. Si fuera editable, alguien podría subir el número sin cambiar el
    // texto (o al revés) y el registro de aceptación dejaría de probar qué texto regía.
    //
    // Al publicar una versión nueva:
    //   1. cambiar el texto en Web/Views/Home/Terminos.cshtml y Privacy.cshtml
    //   2. subir VersionVigente y FechaUltimaActualizacion acá
    // Todos los usuarios volverán a ver la pantalla de aceptación en su próxima petición.
    //
    // Los dos documentos comparten un único número de versión porque se revisan y se aceptan
    // juntos, en un solo acto. Versionarlos por separado exigiría dos pares de columnas en Usuario.
    public static class DocumentosLegales
    {
        public const string VersionVigente = "2.1";

        public const string FechaUltimaActualizacion = "25 de septiembre de 2026";
    }
}
