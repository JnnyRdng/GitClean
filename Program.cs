using GitClean.Commands;
using Spectre.Console.Cli;

namespace GitClean
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp<CleanCommand>()
                .WithDescription("Removes local branches in a git repository that no longer exist on the remote.");

            app.Configure(config =>
            {
                config.SetApplicationName("gitclean");
                config.UseStrictParsing();
            });
            try
            {
                return app.Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
}