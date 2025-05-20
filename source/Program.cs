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
wet exportRequest <requestName> <exportLocation>
*/

CommandApp app = new();
app.Configure(config =>
{
    config.AddCommand<New>("new");
    config.AddCommand<RemoveAt>("removeAt");
    config.AddCommand<Print>("print");
    config.AddCommand<Add>("add");
    config.AddCommand<Reverse>("reverse");
    config.AddCommand<RemoveFrom>("removeFrom");
    config.AddCommand<ExportRequest>("exportRequest");
});
return app.Run(args);

internal sealed class New : Command<New.Settings>
{
    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// The name of the request to be created.
        /// </summary>
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }

        /// <summary>
        /// The hyperlink to the main webpage which will be parsed for links.
        /// </summary>
        [CommandArgument(1, "<link>")]
        public string Link { get; set; }

        /// <summary>
        /// The regex to be used when parsing the main wepbage for links.
        /// </summary>
        [CommandArgument(2, "<regex>")]
        public string Regex { get; set; }
    }

    /// <summary>
    /// Method to execute the Add command.
    /// </summary>
    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
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
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }

        /// <summary>
        /// The index of the element to remove.
        /// </summary>
        [CommandArgument(1, "<index>")]
        public int Index { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
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
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }

        /// <summary>
        /// The starting index of links to print.
        /// </summary>
        [CommandArgument(1, "[start]")]
        public int Start { get; set; }

        /// <summary>
        /// The last index of links to print.
        /// </summary>
        [CommandArgument(2, "[end]")]
        public int End { get; set; }

    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
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
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }

        /// <summary>
        /// The index to add link to.
        /// </summary>
        [CommandArgument(1, "<index>")]
        public int Index { get; set; }

        /// <summary>
        /// The link that is added to the list.
        /// </summary>
        [CommandArgument(2, "<link>")]
        public string Link { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
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
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
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
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }

        /// <summary>
        /// The beginning of the range of links to be removed.
        /// </summary>
        [CommandArgument(1, "<begin>")]
        public int Begin { get; set; }

        /// <summary>
        /// The end of the range of links to be removed.
        /// </summary>
        [CommandArgument(2, "<end>")]
        public int End { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
        creatingRequests.RemoveFrom(settings.Begin, settings.End);
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
        [CommandArgument(0, "<requestName>")]
        public string RequestName { get; set; }

        /// <summary>
        /// The location to export the epub to.
        /// </summary>
        [CommandArgument(1, "<exportLocation>")]
        public string ExportLocation { get; set; }


    }

    public override int Execute(CommandContext context, Settings settings)
    {
        CreatingRequests creatingRequests = new(settings.RequestName);
        creatingRequests.ExportRequest(settings.ExportLocation);
        return 0;
    }

}
