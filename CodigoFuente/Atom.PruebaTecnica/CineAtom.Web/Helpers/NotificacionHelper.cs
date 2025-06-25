using Microsoft.AspNetCore.Mvc;

namespace CineAtom.Web.Helpers
{
    public static class NotificacionHelper
    {
        public static void AgregarNotificacionExito(Controller controller, string mensaje)
        {
            controller.TempData["NotificacionTipo"] = "success";
            controller.TempData["NotificacionTitulo"] = "Éxito";
            controller.TempData["NotificacionMensaje"] = mensaje;
        }

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

        public static void AgregarNotificacionAdvertencia(Controller controller, string mensaje)
        {
            controller.TempData["NotificacionTipo"] = "warning";
            controller.TempData["NotificacionTitulo"] = "Advertencia";
            controller.TempData["NotificacionMensaje"] = mensaje;
        }

        public static void AgregarNotificacionInformacion(Controller controller, string mensaje)
        {
            controller.TempData["NotificacionTipo"] = "info";
            controller.TempData["NotificacionTitulo"] = "Información";
            controller.TempData["NotificacionMensaje"] = mensaje;
        }
    }
}