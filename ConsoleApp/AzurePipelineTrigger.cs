using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApp
{
    // --- Data Models for Azure DevOps API Responses ---
    #region DataModels 
    public class AzDoProject
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class AzDoProjectList
    {
        public int Count { get; set; }
        public List<AzDoProject> Value { get; set; }
    }

    public class AzDoPipeline
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
    }

    public class AzDoPipelineList
    {
        public int Count { get; set; }
        public List<AzDoPipeline> Value { get; set; }
    }

    public class AzDoRepositoryInfo
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string DefaultBranch { get; set; }
    }

    public class AzDoRepositoryList
    {
        public int Count { get; set; }
        public List<AzDoRepositoryInfo> Value { get; set; }
    }

    public class AzDoDesignerJson
    {
        public AzDoRepositoryInfo Repository { get; set; }
    }
    
    // --- FIXED DATA MODELS ---

    /// <summary>
    /// FIXED: This class now correctly models the configuration for both YAML and Classic pipelines.
    /// </summary>
    public class AzDoPipelineConfiguration
    {
        public AzDoDesignerJson DesignerJson { get; set; } // For classic designer pipelines
        public string Type { get; set; }
        public AzDoRepositoryInfo Repository { get; set; } // FIXED: Added this for YAML pipelines
    }
    
    /// <summary>
    /// FIXED: This class is now simplified. It relies solely on the corrected AzDoPipelineConfiguration.
    /// </summary>
    public class AzDoPipelineDefinition
    {
        // FIXED: The top-level repository property was removed.
        public AzDoPipelineConfiguration Configuration { get; set; } 
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    // --- END FIXED DATA MODELS ---

    public class AzDoGitRef
    {
        public string Name { get; set; }
        public string ObjectId { get; set; }
    }

    public class AzDoGitRefList
    {
        public int Count { get; set; }
        public List<AzDoGitRef> Value { get; set; }
    }

    public class AzDoTfvcBranch
    {
        public string Path { get; set; }
        public string Description { get; set; }
    }

    public class AzDoTfvcBranchList
    {
        public int Count { get; set; }
        public List<AzDoTfvcBranch> Value { get; set; }
    }
    #endregion

    public class AzurePipelineTrigger
    {
        private readonly string organization;
        private readonly string pat;
        private static readonly HttpClient client = new HttpClient();

        // --- State Properties ---
        private string selectedProjectId;
        private string selectedProjectName;
        private int selectedPipelineId;
        private string selectedPipelineName;
        private string selectedRepositoryId;
        private string selectedRepositoryName;
        private string selectedRepositoryType;

        public AzurePipelineTrigger(string organization, string pat)
        {
            if (string.IsNullOrWhiteSpace(organization)) throw new ArgumentNullException(nameof(organization));
            if (string.IsNullOrWhiteSpace(pat)) throw new ArgumentNullException(nameof(pat));
            
            this.organization = organization;
            this.pat = pat;

            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{this.pat}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<T> GetAzDoApiAsync<T>(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"API request to {url} failed with status {ex.StatusCode}. Response body: {responseBody}");
                Console.ResetColor();
                throw;
            }

            return JsonConvert.DeserializeObject<T>(responseBody);
        }
        
        public async Task SelectProjectAsync()
        {
            Console.WriteLine("\nFetching projects...");
            var url = $"https://dev.azure.com/{organization}/_apis/projects?api-version=7.0";
            var projectList = await GetAzDoApiAsync<AzDoProjectList>(url);

            if (projectList?.Value == null || projectList.Value.Count == 0)
            {
                throw new InvalidOperationException("No projects found or access denied.");
            }

            Console.WriteLine("Available Projects:");
            for (int i = 0; i < projectList.Value.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {projectList.Value[i].Name}");
            }

            int choice = GetUserChoice(projectList.Value.Count);
            selectedProjectId = projectList.Value[choice - 1].Id;
            selectedProjectName = projectList.Value[choice - 1].Name;
            Console.WriteLine($"Selected Project: {selectedProjectName}");
        }

        public async Task SelectRepositoryAsync()
        {
            if (string.IsNullOrEmpty(selectedProjectId)) throw new InvalidOperationException("Project must be selected first.");

            Console.WriteLine($"\nFetching repositories for project '{selectedProjectName}'...");
            var url = $"https://dev.azure.com/{organization}/{selectedProjectId}/_apis/git/repositories?api-version=7.0";
            var repoList = await GetAzDoApiAsync<AzDoRepositoryList>(url);

            if (repoList?.Value == null || repoList.Value.Count == 0)
            {
                throw new InvalidOperationException("No Git repositories found in this project.");
            }

            Console.WriteLine("Available Repositories:");
            for (int i = 0; i < repoList.Value.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {repoList.Value[i].Name}");
            }

            int choice = GetUserChoice(repoList.Value.Count);
            var selectedRepo = repoList.Value[choice - 1];
            selectedRepositoryId = selectedRepo.Id;
            selectedRepositoryName = selectedRepo.Name;
            selectedRepositoryType = "azureReposGit";
            Console.WriteLine($"Selected Repository: '{selectedRepositoryName}'");
        }
        
        public async Task SelectPipelineForRepoAsync()
        {
            if (string.IsNullOrEmpty(selectedProjectId)) throw new InvalidOperationException("Project not selected.");
            if (string.IsNullOrEmpty(selectedRepositoryId)) throw new InvalidOperationException("Repository not selected.");

            Console.WriteLine($"\nFetching pipelines linked to repository '{selectedRepositoryName}'...");
            
            var allPipelinesUrl = $"https://dev.azure.com/{organization}/{selectedProjectId}/_apis/pipelines?api-version=7.0";
            var allPipelines = await GetAzDoApiAsync<AzDoPipelineList>(allPipelinesUrl);

            if (allPipelines?.Value == null || allPipelines.Value.Count == 0)
            {
                throw new InvalidOperationException("No pipelines found in the project.");
            }

            var matchingPipelines = new List<AzDoPipeline>();
            
            foreach (var pipeline in allPipelines.Value)
            {
                var detailUrl = $"https://dev.azure.com/{organization}/{selectedProjectId}/_apis/pipelines/{pipeline.Id}?api-version=7.0";
                var pipelineDetails = await GetAzDoApiAsync<AzDoPipelineDefinition>(detailUrl);
                
                // --- FIXED LOGIC ---
                // This logic now correctly checks the repository ID in the right place for both YAML and Classic pipelines.
                string repoId = pipelineDetails.Configuration?.Repository?.Id ?? pipelineDetails.Configuration?.DesignerJson?.Repository?.Id;

                if (repoId != null && repoId.Equals(selectedRepositoryId, StringComparison.OrdinalIgnoreCase))
                {
                    matchingPipelines.Add(pipeline);
                }
            }

            if (matchingPipelines.Count == 0)
            {
                throw new InvalidOperationException($"No pipelines found linked to the repository '{selectedRepositoryName}'.");
            }
            
            Console.WriteLine("Available Pipelines:");
            for (int i = 0; i < matchingPipelines.Count; i++)
            {
                var p = matchingPipelines[i];
                string displayName = string.IsNullOrEmpty(p.Folder) ? p.Name : $"{p.Folder.TrimStart('\\')}\\{p.Name}";
                Console.WriteLine($"{i + 1}. {displayName}");
            }

            int choice = GetUserChoice(matchingPipelines.Count);
            selectedPipelineId = matchingPipelines[choice - 1].Id;
            selectedPipelineName = matchingPipelines[choice - 1].Name;
            Console.WriteLine($"Selected Pipeline: {selectedPipelineName}");
        }

        public async Task<string> SelectBranchAsync()
        {
            if (string.IsNullOrEmpty(selectedRepositoryId)) throw new InvalidOperationException("Repository not selected.");

            Console.WriteLine($"\nFetching branches for repository '{selectedRepositoryName}'...");
            var url = $"https://dev.azure.com/{organization}/{selectedProjectId}/_apis/git/repositories/{selectedRepositoryId}/refs?filter=heads&api-version=7.0";
            var branchList = await GetAzDoApiAsync<AzDoGitRefList>(url);

            if (branchList?.Value == null || branchList.Value.Count == 0)
            {
                throw new InvalidOperationException($"No branches found in repository '{selectedRepositoryName}'.");
            }

            Console.WriteLine("Available Branches:");
            for (int i = 0; i < branchList.Value.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {branchList.Value[i].Name.Replace("refs/heads/", "")}");
            }

            int choice = GetUserChoice(branchList.Value.Count);
            string selectedBranch = branchList.Value[choice - 1].Name;
            Console.WriteLine($"Selected Branch: {selectedBranch.Replace("refs/heads/", "")}");
            return selectedBranch;
        }

        public async Task<string> SelectTagAsync()
        {
            if (string.IsNullOrEmpty(selectedRepositoryId)) throw new InvalidOperationException("Repository not selected.");
            
            Console.WriteLine($"\nFetching tags for repository '{selectedRepositoryName}'...");
            var url = $"https://dev.azure.com/{organization}/{selectedProjectId}/_apis/git/repositories/{selectedRepositoryId}/refs?filter=tags&api-version=7.0";
            var tagList = await GetAzDoApiAsync<AzDoGitRefList>(url);

            if (tagList?.Value == null || tagList.Value.Count == 0)
            {
                throw new InvalidOperationException($"No tags found in repository '{selectedRepositoryName}'.");
            }

            Console.WriteLine("Available Tags:");
            for (int i = 0; i < tagList.Value.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tagList.Value[i].Name.Replace("refs/tags/", "")}");
            }

            int choice = GetUserChoice(tagList.Value.Count);
            string selectedTag = tagList.Value[choice - 1].Name;
            Console.WriteLine($"Selected Tag: {selectedTag.Replace("refs/tags/", "")}");
            return selectedTag;
        }

        public async Task TriggerPipelineAsync(string envName, string accessToken, string sourceRef)
        {
            if (string.IsNullOrEmpty(sourceRef)) throw new InvalidOperationException("Source ref (branch or tag) must be selected.");
            if (selectedPipelineId == 0) throw new InvalidOperationException("Pipeline must be selected.");

            var uri = $"https://dev.azure.com/{organization}/{selectedProjectName}/_apis/pipelines/{selectedPipelineId}/runs?api-version=7.1-preview.1";

            var payload = new
            {
                resources = new { repositories = new { self = new { refName = sourceRef } } },
                templateParameters = new { envName = envName, accessToken = accessToken }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            string simpleRefName = sourceRef.StartsWith("refs/heads/") 
                ? sourceRef.Replace("refs/heads/", "") 
                : sourceRef.Replace("refs/tags/", "");

            Console.WriteLine($"\nTriggering pipeline '{selectedPipelineName}' on ref '{simpleRefName}'...");
            var response = await client.PostAsync(uri, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Pipeline triggered successfully.");
                Console.ResetColor();
                Console.WriteLine($"Response: {responseBody}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to trigger pipeline. Status: {response.StatusCode}");
                Console.WriteLine($"Error details: {responseBody}");
                Console.ResetColor();
            }
        }

        private int GetUserChoice(int maxOption)
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