using Microsoft.AspNetCore.Mvc.Rendering;

/* 
    If you want to add the active class on the li element when you are in
    the “Home” controller on the “Index” action, you will need the following
    code in your view:

        <li class="nav-item @Html.ActiveClass("Home", "Index")">
            <a class="nav-link" href="/Home/Index">Home</a>
        </li>

    But let’s say you want to trigger the selected class when you are on the
    “Index” action of either the “Home” or “Contact” controller. We can do
    so with the next piece of code:

        <li class="nav-item @Html.ActiveClass("Home,Contact", "Index", "selected")">
            <a class="nav-link" href="/Home/Index">Home</a>
        </li>

    **NOTE: I would highly recommend adding the @using Your.Extension.NameSpace to
            your _ViewImports.cshtml file, so you don’t have to add it to every
            other view.
*/


namespace BugTrackerMVC.Helper
{
    public static class MvcExtensions
    {
        public static string ActiveClass(this IHtmlHelper htmlHelper, string controllers = null!, string actions = null!, string cssClass = "active")
        {
            var currentController = htmlHelper?.ViewContext.RouteData.Values["controller"] as string;
            var currentAction = htmlHelper?.ViewContext.RouteData.Values["action"] as string;

            var acceptedControllers = (controllers ?? currentController ?? "").Split(',');
            var acceptedActions = (actions ?? currentAction ?? "").Split(',');

            return acceptedControllers.Contains(currentController) && acceptedActions.Contains(currentAction)
                ? cssClass
                : "";
        }

        public static string DeTag(this IHtmlHelper htmlHelper, string taggedText)
        {
			var tagRx = new System.Text.RegularExpressions.Regex("<[^>]*>");
			taggedText = tagRx.Replace(taggedText, "");
            return taggedText;
		}
    }
}
