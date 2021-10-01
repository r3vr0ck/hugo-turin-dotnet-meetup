## **Static website build & deployment using YAML pipeline**
The **documentation.yml** file is responsible for the build and deployment of the static website, 
which is automatically triggered when creating a pull request or when completing it and merging on the **main** branch.

Make sure your **main** branch has a Build Validation policy in place, it can be checked from your Repo settings -> branch policies and from there you can pick the documentation.yml

![Branch policy](/markdown-guidance/branch-policy.png)


## **BUILD stage**
Regarding the BUILD stage, it is important to note that after running the **npm** task with the **npm install** command, it is necessary to run the 
**hugo.exe** which can be found in the **documentation/deploy/** folder, in order to generate the static website from our markdown and assets files.
Everything will be then copied into the **Build.ArtifactStagingDirectory\docs-assets** and then published with the **PublishBuildArtifacts** task.

## **DOC_WEBSITE_PR stage**
The DOC_WEBSITE_PR stage is triggered when creating a pull request for the **main** branch and it is responsible for uploading (via **FtpUpload@2** task) our artifacts in a new folder marked with the Pull Request Id, in the wwwroot directory of the .NET core documentation host app.
Ex. /site/wwwroot/wwwroot/pr-$(System.PullRequest.PullRequestId)/

![PR FTP Task](/markdown-guidance/pr-yaml.png)

![Variables](/markdown-guidance/variables.png)

The FTP variables of an Azure Web App can be found in the **Get publish profile** link of the Web App itself.

Please keep in mind that when declaring your variables in Azure DevOps, a secret variable such as FTP_PASSWORD in order to be used in the FTPUpload task, has to be declared as env variable in your stage. 

![FTP Pwd](/markdown-guidance/ftp-pwd.png)


The final task of our Pull Request stage is a powershell script that leveraging the Azure DevOps API, will create a comment with the link to the 'preview environment' of our PR.
More informations can be found [here](https://docs.microsoft.com/en-us/rest/api/azure/devops/git/pull%20request%20threads?view=azure-devops-rest-5.1)

**IMPORTANT! Please note that the build service in Azure DevOps has to be allowed to contribute to pull requests**

From Project settings -> Repositories -> your-repo  -> Security 

Select your build user and set **Allow** to both **Contribute** and **Contribute to Pull Requests**

![Contribute](/markdown-guidance/contribute.png)

## **DEVELOPMENT stage**
The DEVELOPMENT stage will be triggered when merging in the **main** branch as a result of a completed pull request. Please note that a path filter is in place and only changes within the **documentation** folder will trigger it.

![Path filter](/markdown-guidance/path-filter.png)

This stage uses the same **FtpUpload@2** used before and will upload our artifacts in the wwwroot folder of the .NET core documentation host app.