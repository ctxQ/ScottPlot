﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScottPlot.Cookbook.Site
{
    /// <summary>
    /// The cookbook index page has links to recipes located on individual category pages
    /// </summary>
    public class IndexPage : Page
    {
        public IndexPage(string cookbookSiteFolder, string resourceFolder) : base(cookbookSiteFolder, resourceFolder) { }

        public void AddLinksToRecipes()
        {
            foreach (var stuff in Locate.GetCategorizedRecipes())
                AddRecipeGroup(stuff.Key, stuff.Value);

            DivStart("categorySection");
            AddGroupHeader("Miscellaneous");
            AddHTML("<div style='margin-left: 1em;'><a href='all_recipes.html'>Single page with all recipes</a></div>");
            DivEnd();
        }

        private void AddTableRow(string[] columnNames, bool header = false)
        {
            string trStyle = header ? "style='font-weight: 600; background-color: #f6f8fa;'" : "";
            AddHTML($"<tr {trStyle}>");
            foreach (var name in columnNames)
                AddHTML($"<td>{name}</td>");
            AddHTML("</tr>");
        }

        public void AddPlotApiTableWithoutPlottables(XmlDoc xd, MethodInfo[] plotMethods)
        {
            string linkPrefix = "api-plot.html";
            AddGroupHeader("The Plot API");
            AddHTML("Interact with the Plot module to configure styling or behavior.");

            AddHTML("<table>");
            AddTableRow(new string[] { "Method", "Summary" }, true);

            foreach (MethodInfo mi in plotMethods)
            {
                bool isAddPlottableMethod = mi.Name.StartsWith("Add") && mi.Name.Length > 3 && !mi.Name.Contains("Axis");
                if (isAddPlottableMethod)
                    continue;
                string summary = xd.GetSummary(mi);

                // format differently if it's a real method or an auto-property
                string itemName = mi.Name;
                if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))
                    itemName = itemName.Replace("get_", "").Replace("set_", "");
                else
                    itemName += "()";

                string url = Sanitize(itemName);
                string name = $"<a href='{linkPrefix}#{url}'><strong>{itemName}</strong></a>";
                AddTableRow(new string[] { name, summary });
            }

            AddHTML("</table>");
        }

        public void AddPlottableApiTable(XmlDoc xd)
        {
            AddGroupHeader("Plottable Types");
            AddHTML("This table lists the types of plottable objects that can be added to a plot. " +
            "Many of these plottable types have helper methods to easily create, customize, and add them to the plot. " +
            "Public methods, properties, and fields of plottables can be used to customize their behavior and styling.");

            AddHTML("<table>");
            AddTableRow(new string[] { "Method", "Summary" }, true);

            foreach (Type plottableType in Locate.GetPlottableTypes())
            {
                string summary = xd.GetSummary(plottableType);
                string name = Locate.TypeName(plottableType);
                string url = Locate.TypeName(plottableType, urlSafe: true);
                string html = $"<a href='api-plottable-{url}.html'><strong>{name}</strong></a>";
                AddTableRow(new string[] { html, summary });
            }

            AddHTML("</table>");
        }

        public void AddPlotApiTablePlottables(XmlDoc xd, MethodInfo[] plotMethods, string linkPrefix = "")
        {
            AddGroupHeader("Helper Methods for Adding Plottables");
            AddHTML("These methods make it easy to add specific plottable to the plot. ");
            AddHTML("Common styling configuration is available as optional arguments, and these methods return " +
                "the plottable they create so you can interact with it directly to further customize styling " +
                "or udpate data after adding it to the plot.");

            AddHTML("<table>");
            AddTableRow(new string[] { "Method", "Summary" }, true);

            foreach (MethodInfo mi in plotMethods)
            {
                bool isAddPlottableMethod = mi.Name.StartsWith("Add") && mi.Name.Length > 3 && !mi.Name.Contains("Axis");
                if (!isAddPlottableMethod)
                    continue;
                string summary = xd.GetSummary(mi);

                // format differently if it's a real method or an auto-property
                string itemName = mi.Name;
                if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))
                    itemName = itemName.Replace("get_", "").Replace("set_", "");
                else
                    itemName += "()";

                string url = Sanitize(mi.Name);
                string name = $"<a href='{linkPrefix}#{url}'><strong>{itemName}</strong></a>";
                AddTableRow(new string[] { name, summary });
            }

            AddHTML("</table>");
        }

        public void AddPlotApiDetails(XmlDoc xd, MethodInfo[] plotMethods)
        {
            foreach (MethodInfo mi in plotMethods)
            {
                string name = mi.Name;
                string url = Sanitize(name);
                string summary = xd.GetSummary(mi);
                string returnType = XmlDoc.PrettyType(mi.ReturnType);
                string signature = XmlDoc.PrettySignature(mi);

                // format differently if it's a real method or an auto-property
                string itemName = mi.Name;
                if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))
                    itemName = itemName.Replace("get_", "").Replace("set_", "");
                else
                    itemName += "()";

                AddGroupHeader(itemName, marginTop: "4em");

                AddHTML($"<div><strong>Summary:</strong></div>");
                AddHTML($"<ul><li>{summary}</li></ul>");

                bool isAutoProperty = mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_");
                if (isAutoProperty)
                {
                    // this method is an auto-property describing a field

                    signature = signature.Replace("()", "").Replace("get_", "").Replace("set_", "");
                    AddHTML($"<div><strong>Field:</strong></div>");
                    AddHTML($"<ul><li><code>{signature}</code></li></ul>");
                }
                else
                {
                    // this method is a traditional method with inputs and outputs

                    AddHTML($"<div><strong>Parameters:</strong></div>");
                    AddHTML("<ul>");
                    foreach (var p in mi.GetParameters())
                        AddHTML($"<li><code>{XmlDoc.PrettyType(p.ParameterType)}</code> {p.Name}</li>");
                    AddHTML("</ul>");

                    AddHTML($"<div><strong>Returns:</strong></div>");
                    AddHTML($"<ul><li><code>{returnType}</code></li></ul>");

                    AddHTML($"<div><strong>Signature:</strong></div>");
                    AddHTML($"<ul><li><code>{signature}</code></li></ul>");
                }
            }
        }

        private void AddRecipeGroup(string groupName, IRecipe[] recipes)
        {
            DivStart("categorySection");
            AddGroupHeader(groupName);
            AddRecipeLinks(recipes);
            AddRecipeThumbnails(recipes);
            DivEnd();
        }

        private void AddGroupHeader(string title, string marginTop = "1em")
        {
            string anchor = Sanitize(title);
            SB.AppendLine($"<h2 style='margin-top: {marginTop};'>");
            SB.AppendLine($"<a href='#{anchor}' name='{anchor}' style='color: black; text-decoration: none; font-weight: 600;'>{title}</a>");
            SB.AppendLine($"</h2>");
        }

        private void AddRecipeLinks(IRecipe[] recipes)
        {
            SB.Append("<div style='margin-left: 1em;'>");
            foreach (IRecipe recipe in recipes)
            {
                string categoryPageName = $"{Sanitize(recipe.Category)}{ExtPage}";
                string recipeUrl = $"{categoryPageName}#{recipe.ID}".ToLower();
                SB.AppendLine($"<p><a href='{recipeUrl}' style='font-weight: 600;'>{recipe.Title}</a> - {recipe.Description}</p>");
            }
            SB.Append("</div>");
        }

        private void AddRecipeThumbnails(IRecipe[] recipes)
        {
            foreach (IRecipe recipe in recipes)
            {
                string categoryPageName = $"{Sanitize(recipe.Category)}{ExtPage}";
                string recipeUrl = $"{categoryPageName}#{recipe.ID}".ToLower();
                string imageUrl = $"source/{recipe.ID}{ExtThumb}".ToLower();
                SB.AppendLine($"<a href='{recipeUrl}'><img src='{imageUrl}' style='padding: 10px;'/></a>");
            }
        }

        public void AddPlottableDetails(XmlDoc xd, Type plottableType)
        {
            string typeName = Locate.TypeName(plottableType);
            string typeUrl = Locate.TypeName(plottableType, urlSafe: true);

            AddHTML($"<div><strong>This page describes <code>ScottPlot.Plottable.{typeName}</code></strong></div>");

            string classSummary = xd.GetSummary(plottableType);
            AddHTML($"<div><strong>Summary:</strong> {classSummary}</div>");

            AddHTML($"<blockquote>" +
                "⚠️ <strong>Only public methods are shown below.</strong> " +
                "This plot type likely has properties and fields to customize its behavior and styling, " +
                "but they do not appear on this page. " +
                "</blockquote>");

            // TODO: fix URLs for generic types
            string apimundoUrl =
                "https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/ScottPlot/versions/latest?" +
                "tab=types&namespace=ScottPlot.Plottable&type=ScottPlot.Plottable." + typeName;
            AddHTML($"<div>💡 Read more on Apimundo: <a href='{apimundoUrl}'>ScottPlot.Plottable.{typeName}</a></div>");

            foreach (MethodInfo mi in Locate.GetNotablePlottableMethods(plottableType))
            {
                string methodSummary = xd.GetSummary(mi);
                string returnType = XmlDoc.PrettyType(mi.ReturnType);
                string signature = XmlDoc.PrettySignature(mi);

                AddGroupHeader(mi.Name + "()");
                AddHTML($"<div><strong>Summary:</strong> {methodSummary}</div>");

                AddHTML($"<div><strong>Parameters:</strong></div>");
                AddHTML("<ul>");
                foreach (var p in mi.GetParameters())
                    AddHTML($"<li><code>{XmlDoc.PrettyType(p.ParameterType)}</code> {p.Name}</li>");
                AddHTML("</ul>");

                AddHTML($"<div><strong>Returns:</strong></div>");
                AddHTML($"<ul><li><code>{returnType}</code></li></ul>");

                AddHTML($"<div><strong>Signature:</strong></div>");
                AddHTML($"<ul><li><code>{signature}</code></li></ul>");
            }
        }
    }
}
