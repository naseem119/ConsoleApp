namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Azure DevOps Pipeline Trigger ---");

            // IMPORTANT: Replace with your actual Organization and Personal Access Token (PAT)
            var organization = "zeva";
            var pat = "1FUxjBjpiRfb5ykIYwwdOp3K828V1hlb1B8ONkE4R6nT2O4pBWNJJQQJ99BFACAAAAA7UrhMAAASAZDO3OYq"; // <-- IMPORTANT: Replace with a valid PAT with `Build: Read & Execute` and `Code: Read` scopes.

            // IMPORTANT: Replace with your actual parameters for the pipeline
            var envName = "dev";
            var pipelineAccessToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJPQTdyYjV2d1FnZ0h0bEctcnRjUkRtbGd3bmFSaF90eHNSdUdtRElJdm9NIn0.eyJleHAiOjE3NDkwMjk0NTQsImlhdCI6MTc0OTAyOTMzNCwiYXV0aF90aW1lIjoxNzQ5MDIzMTI4LCJqdGkiOiJmZjZhOGMyZS03NTlhLTQ0NjgtYWZkOS0zMTFmYmQyMjIyZTgiLCJpc3MiOiJodHRwczovL2tleWNsb2FrOjg0NDMvcmVhbG1zL21hc3RlciIsImF1ZCI6WyJtYXN0ZXItcmVhbG0iLCJhY2NvdW50Il0sInN1YiI6ImRhZjA2NTMzLTI0NGItNDU2Mi1iOTNkLTA5ZDlhMGYwNTlhYiIsInR5cCI6IkJlYXJlciIsImF6cCI6ImNvZGVsb2NrZXIiLCJzZXNzaW9uX3N0YXRlIjoiMmRlYTA2NjQtZjE3MS00MjhmLWI5MzQtNTQ4ZmVjZGNiYmFiIiwiYWNyIjoiMCIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwczovL2xvY2FsaG9zdDo4MDgyIiwiaHR0cHM6Ly9jb2RlbG9ja2VyOjMwMDAyIiwiaHR0cHM6Ly8xOTIuMTY4LjQ5LjI6MzAwMDIiLCJodHRwczovL2xvY2FsaG9zdDozMDAwMiIsImh0dHBzOi8vMTAuMC4xMjguMTA4OjMwMDAyIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJjcmVhdGUtcmVhbG0iLCJkZWZhdWx0LXJvbGVzLW1hc3RlciIsIm9mZmxpbmVfYWNjZXNzIiwiYWRtaW4iLCJkZXZlbG9wZXIiLCJ1bWFfYXV0aG9yaXphdGlvbiJdfSwicmVzb3VyY2VfYWNjZXNzIjp7Im1hc3Rlci1yZWFsbSI6eyJyb2xlcyI6WyJ2aWV3LXJlYWxtIiwidmlldy1pZGVudGl0eS1wcm92aWRlcnMiLCJtYW5hZ2UtaWRlbnRpdHktcHJvdmlkZXJzIiwiaW1wZXJzb25hdGlvbiIsImNyZWF0ZS1jbGllbnQiLCJtYW5hZ2UtdXNlcnMiLCJxdWVyeS1yZWFsbXMiLCJ2aWV3LWF1dGhvcml6YXRpb24iLCJxdWVyeS1jbGllbnRzIiwicXVlcnktdXNlcnMiLCJtYW5hZ2UtZXZlbnRzIiwibWFuYWdlLXJlYWxtIiwidmlldy1ldmVudHMiLCJ2aWV3LXVzZXJzIiwidmlldy1jbGllbnRzIiwibWFuYWdlLWF1dGhvcml6YXRpb24iLCJtYW5hZ2UtY2xpZW50cyIsInF1ZXJ5LWdyb3VwcyJdfSwiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJvcGVuaWQgZW1haWwgcHJvZmlsZSIsInNpZCI6IjJkZWEwNjY0LWYxNzEtNDI4Zi1iOTM0LTU0OGZlY2RjYmJhYiIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwibmFtZSI6IkFkbWluaXN0cmF0b3IiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJhZG1pbmlzdHJhdG9yIiwiZmFtaWx5X25hbWUiOiJBZG1pbmlzdHJhdG9yIiwiZW1haWwiOiJhZG1pbmlzdHJhdG9yQGRuYi5sYWIifQ.DF9KeNN5GGaZFhM7DXoTfV2LSXT0FNiAq_5-kYTnJPnjlGzHCeslNGnUptAb9aLYq8DmuNWz5tfDV6IRUKhSbkmgKz13Vvh1G1xk41UFSHx7r8JUv6AJxg_JQKxG1zalwxpbMofH97ASVWOlysinIVMORzRxpYl4ANCVxy_oHjnKAZOE2eJ1sk8fNoZuMyCWKKX49g-YZfFlPFyeT9dwl0Yfat2KmKpavy48U4C6IB95ps5-1kXNsmTDlSndyxQVWmZhwj61QPR1ltLq2V0-yh24flo3WjtoYNmh255kxvSswyXkfVdkLoOluSLDfzpmYJbyZompibAP541-DHSLUQ"; // <-- IMPORTANT: Replace with a valid Token


            try
            {
                var trigger = new AzurePipelineTrigger(organization, pat);

                // --- MODIFIED WORKFLOW ---
                await trigger.SelectProjectAsync();
                await trigger.SelectRepositoryAsync();
                await trigger.SelectPipelineForRepoAsync(); // Uses the corrected logic to find pipelines

                // --- CHOOSE BETWEEN BRANCH OR TAG ---
                Console.WriteLine("\nTrigger from a:");
                Console.WriteLine("1. Branch");
                Console.WriteLine("2. Tag");
                int triggerChoice = GetUserChoice(2);

                string sourceRef = string.Empty;

                if (triggerChoice == 1)
                {
                    sourceRef = await trigger.SelectBranchAsync();
                }
                else
                {
                    sourceRef = await trigger.SelectTagAsync();
                }
                // --- END SECTION ---

                await trigger.TriggerPipelineAsync(envName: envName, accessToken: pipelineAccessToken, sourceRef: sourceRef);
            }
            catch (ArgumentNullException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Configuration error: {ex.Message}");
                Console.ResetColor();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Operation error: {ex.Message}");
                Console.ResetColor();
            }
            catch (NotSupportedException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unsupported operation: {ex.Message}");
                Console.ResetColor();
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"API request failed: {ex.Message}");
                if (ex.StatusCode.HasValue)
                {
                    Console.WriteLine($"Status Code: {ex.StatusCode}");
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Helper method to get a valid user choice from the console.
        /// </summary>
        private static int GetUserChoice(int maxOption)
        {
            int choice;
            while (true)
            {
                Console.Write($"Enter your choice (1-{maxOption}): ");
                if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= maxOption)
                {
                    break;
                }
                Console.WriteLine("Invalid input. Please try again.");
            }
            return choice;
        }
    }
}