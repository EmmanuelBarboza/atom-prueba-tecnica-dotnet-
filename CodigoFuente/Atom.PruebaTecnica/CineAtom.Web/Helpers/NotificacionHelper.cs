using Microsoft.AspNetCore.Mvc;

namespace CineAtom.Web.Helpers
{
    /// <summary>
    /// Clase para agregar mensajes temporales tipo notificacion usando TempData
    /// Se usan para mostrar alertas al usuario en la interfaz
    /// </summary>
    public static class NotificacionHelper
    {
        /// <summary>
        /// Muestra un mensaje de exito
        /// Ideal cuando todo sale bien
        /// </summary>
        public static void AgregarNotificacionExito(Controller controller, string mensaje)
        {
            controller.TempData["NotificacionTipo"] = "success";
            controller.TempData["NotificacionTitulo"] = "Exito";
            controller.TempData["NotificacionMensaje"] = mensaje;
        }

        /// <summary>
        /// Muestra un mensaje de error
        /// Se puede incluir un detalle tecnico
        /// </summary>
        public static void AgregarNotificacionError(Controller controller, string mensaje, string detalle = null)
        {
            controller.TempData["NotificacionTipo"] = "error";
            controller.TempData["NotificacionTitulo"] = "Error";
            controller.TempData["NotificacionMensaje"] = mensaje;
            if (!string.IsNullOrEmpty(detalle))
            {
                controller.TempData["NotificacionDetalle"] = detalle;
            }
        }

        /// <summary>
        /// Muestra una advertencia
        /// Para situaciones no criticas pero importantes
        /// </summary>
        public static void AgregarNotificacionAdvertencia(Controller controller, string mensaje)
        {
            controller.TempData["NotificacionTipo"] = "warning";
            controller.TempData["NotificacionTitulo"] = "Advertencia";
            controller.TempData["NotificacionMensaje"] = mensaje;
        }

        /// <summary>
        /// Muestra un mensaje informativo
        /// Para datos generales o aclaraciones
        /// </summary>
        public static void AgregarNotificacionInformacion(Controller controller, string mensaje)
        {
            controller.TempData["NotificacionTipo"] = "info";
            controller.TempData["NotificacionTitulo"] = "Informacion";
            controller.TempData["NotificacionMensaje"] = mensaje;
        }
    }
}
