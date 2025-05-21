using System.ComponentModel;
using System.Text.RegularExpressions;
using source.Create_Requests;
using Spectre.Console.Cli;

/*
wet new <requestName> <link> <regex>
wet removeAt <requestName> <index>
wet print <requestName> [start] [end]
wet add <requestName> <index> <link>
wet reverse <requestName>
wet removeFrom <requestName> <begin> <end>
wet clear <requestName>
wet exportRequest <requestName> <exportLocation>
wet listRequests
*/

CommandApp app = new();
app.Configure(static config =>
{
    _ = config.AddCommand<New>("new");
    _ = config.AddCommand<RemoveAt>("removeAt");
    _ = config.AddCommand<Print>("print");
    _ = config.AddCommand<Add>("add");
    _ = config.AddCommand<Reverse>("reverse");
    _ = config.AddCommand<RemoveFrom>("removeFrom");
    _ = config.AddCommand<ExportRequest>("exportRequest");
});
return app.Run(args);

internal sealed class New : Command<New.Settings>
{
    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be created.
        /// </summary>
        [Description("The name of the request to be created.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }

        /// <summary>
        /// The hyperlink to the main webpage which will be parsed for links.
        /// </summary>
        [Description("The hyperlink to the main webpage which will be parsed for links.")]
        [CommandArgument(1, "<link>")]
        public required string Link { get; set; }

        /// <summary>
        /// The regex to be used when parsing the main wepbage for links.
        /// </summary>
        [Description("The regex to be used when parsing the main wepbage for links.")]
        [CommandArgument(2, "<regex>")]
        public required string Regex { get; set; }

        /// <summary>
        /// Turn on trace for tor browser.
        /// </summary>
        [Description("Turn on trace for tor browser.")]
        [CommandOption("-t|--trace")]
        [DefaultValue(false)]
        public bool Trace { get; set; }
    }

    /// <summary>
    /// Method to execute the Add command.
    /// </summary>
    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, settings.Trace);
        Regex regex = new(settings.Regex);
        creatingRequests.RequestLinks(settings.Link, regex);
        return 0;
    }
}

internal sealed class RemoveAt : Command<RemoveAt.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }

        /// <summary>
        /// The index of the element to remove.
        /// </summary>
        [Description("The index of the element to remove.")]
        [CommandArgument(1, "<index>")]
        public int Index { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, false);
        creatingRequests.RemoveAt(settings.Index);
        return 0;
    }
}

internal sealed class Print : Command<Print.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }

        /// <summary>
        /// The starting index of links to print.
        /// </summary>
        [Description("The starting index of links to print.")]
        [CommandArgument(1, "[start]")]
        public int Start { get; set; }

        /// <summary>
        /// The last index of links to print.
        /// </summary>
        [Description("The last index of links to print.")]
        [CommandArgument(2, "[end]")]
        public int End { get; set; }

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, false);
        if (context.Arguments.Count == 4)
        {
            creatingRequests.Print(settings.Start, settings.End);
            return 0;
        }
        creatingRequests.Print();
        return 0;
    }

}

internal sealed class Add : Command<Add.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }

        /// <summary>
        /// The index to add link to.
        /// </summary>
        [Description("The index to add link to.")]
        [CommandArgument(1, "<index>")]
        public int Index { get; set; }

        /// <summary>
        /// The link that is added to the list.
        /// </summary>
        [Description("The link that is added to the list.")]
        [CommandArgument(2, "<link>")]
        public required string Link { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, false);
        creatingRequests.Add(settings.Index, settings.Link);
        return 0;
    }
}

internal sealed class Reverse : Command<Reverse.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, false);
        creatingRequests.Reverse();
        return 0;
    }
}

internal sealed class RemoveFrom : Command<RemoveFrom.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }

        /// <summary>
        /// The beginning of the range of links to be removed.
        /// </summary>
        [Description("The beginning of the range of links to be removed.")]
        [CommandArgument(1, "<begin>")]
        public int Begin { get; set; }

        /// <summary>
        /// The end of the range of links to be removed.
        /// </summary>
        [Description("The end of the range of links to be removed.")]
        [CommandArgument(2, "<end>")]
        public int End { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, false);
        creatingRequests.RemoveFrom(settings.Begin, settings.End);
        return 0;
    }
}

internal sealed class Clear : Command<Clear.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, false);
        creatingRequests.Clear();
        return 0;
    }


}

internal sealed class ExportRequest : Command<ExportRequest.Settings>
{

    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be modified.
        /// </summary>
        [Description("The name of the request to be modified.")]
        [CommandArgument(0, "<requestName>")]
        public required string RequestName { get; set; }

        /// <summary>
        /// The location to export the epub to.
        /// </summary>
        [Description("The location to export the epub to.")]
        [CommandArgument(1, "<exportLocation>")]
        public required string ExportLocation { get; set; }

        /// <summary>
        /// Turn on trace for tor browser.
        /// </summary>
        [Description("Turn on trace for tor browser.")]
        [CommandOption("-t|--trace")]
        [DefaultValue(false)]
        public bool Trace { get; set; }


    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName, settings.Trace);
        creatingRequests.ExportRequest(settings.ExportLocation);
        return 0;
    }

}

internal sealed class ListRequests : Command<ListRequests.Settings>
{

    public sealed class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests.ListRequests();
        return 0;
    }
}
