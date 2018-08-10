#pragma checksum "c:\Users\w1shl\Dropbox\VisualStudioProjects\Projects\WebAppRfc\WebAppRfc\Views\Home\AngularRoute.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "6be4fabf5bb849afabc5a6514809ca7b5d9127cc"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_AngularRoute), @"mvc.1.0.view", @"/Views/Home/AngularRoute.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Home/AngularRoute.cshtml", typeof(AspNetCore.Views_Home_AngularRoute))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "c:\Users\w1shl\Dropbox\VisualStudioProjects\Projects\WebAppRfc\WebAppRfc\Views\_ViewImports.cshtml"
using WebAppRfc;

#line default
#line hidden
#line 2 "c:\Users\w1shl\Dropbox\VisualStudioProjects\Projects\WebAppRfc\WebAppRfc\Views\_ViewImports.cshtml"
using WebAppRfc.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"6be4fabf5bb849afabc5a6514809ca7b5d9127cc", @"/Views/Home/AngularRoute.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a90c131eb4fc474f8be073ba9427ab21d9f6bfbb", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_AngularRoute : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/js/angular_app.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(0, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 2 "c:\Users\w1shl\Dropbox\VisualStudioProjects\Projects\WebAppRfc\WebAppRfc\Views\Home\AngularRoute.cshtml"
  
    ViewData["Title"] = "View";

#line default
#line hidden
            BeginContext(42, 1510, true);
            WriteLiteral(@"<div class=""container"">
    <header ng-controller=""MainCtrl as main"" ng-init=""main.GetBase()"">
        <ul class=""nav nav-tabs"">
            <li class=""nav-item active"">
                <a ng-class=""{ active: main.myFactory.isActive('/')}"" class=""nav-link"" href=""#!/"">Home</a>
            </li>
            <li  class=""nav-item"">
                <a ng-class=""{ active: main.myFactory.isActive('/addnew')}"" class=""nav-link"" href=""#!/addnew""> Add new device</a>
            </li>
            <li class=""nav-item"">
                <a ng-class=""{ active: main.myFactory.isActive('/rooms')}"" class=""nav-link"" href=""#!/rooms""> Eddit rooms</a>
            </li>
            <li ng-click=""main.ShowBaseFromMainCtrl()"" class=""nav-item"">
                <a class=""nav-link"" href="""">Test factory</a>
            </li>
            <li ng-click=""main.myFactory.AddTest()"" class=""nav-item"">
                <a class=""nav-link"" href="""">Add item</a>
            </li>
            <li class=""nav-item dropdown"">
          ");
            WriteLiteral(@"      <a class=""nav-link dropdown-toggle"" data-toggle=""dropdown"" href=""#"">Dropdown</a>
                <div class=""dropdown-menu"">
                    <a class=""dropdown-item active"" href="""">Home</a>
                    <a class=""dropdown-item"" href="""">Profile</a>
                    <a class=""dropdown-item"" href="""">Messages</a>
                </div>
            </li>
        </ul>
    </header>
<section>
    <ng-view></ng-view>
</section>
<footer></footer>
</div>

");
            EndContext();
            DefineSection("Scripts", async() => {
                BeginContext(1570, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(1576, 43, false);
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "af439ea54ce64b758858810d8ef9d3fb", async() => {
                }
                );
                __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                EndContext();
                BeginContext(1619, 2, true);
                WriteLiteral("\r\n");
                EndContext();
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
