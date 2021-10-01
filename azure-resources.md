## **Azure resources**
Required before beginning: 
Know environment the resources you have to create belong to
Know the Azure Region you want to deploy the resources
Know the Azure Subscription you have to create the resources (use the same as the RG).

**1) Create the Resource Group (RG)**
- From within portal.azure.com click on **Resource Groups**
- Click **Create**
- Choose a meaningful name for it, we went for **hugo-demo**
- Select the appropriate azure subscribtion and the Region, then click on **Review + create**

**2) Create App Service Plan**

In our case we named it **hugo-demo-plan**

**Choose the proper pricing tier (depending on the environment):**
For our POC we used the free one F1
 
Choose Operating System : Windows.
Choose correct region 
Choose correct RG

**3) Create App Service to host main-site**
From within your RG click on **+ Create** and select **Web App**
Set a meaningful name for it, in our POC we went for **hugo-demo-app**
Set Publish : "code"
Set Run time stack : .NET 5
Choose correct region 
Choose correct RG
Choose the App service plan the one created above - **IMPORTANT** make sure you choose the App service plan created in the same RG
After creation : 
Scale out according to environment, but minimum recommended 2 in any environment (to spot in advance issues that shows up in a web farm configuration only). 
Configuration -> General Settings : Set always on to true 
Configuration -> Application settings : Add 


![Hugo Demo App](/markdown-guidance/demo-app-azure.png)

ASPNETCORE_ENVIRONMENT = **\<environmentName\>** : \<env\> can be used, but it's likely better to pick up a more meaningful code for the environment, instead of the single acronym value. 
This value MUST be in sync with the Environment name of the release pipeline AND match the config file names appsettings.**\<environmentName\>**.json.

Setting of ASPNETCORE_ENVIRONMENT is required ONLY if this value is not set during the release pipeline using azure cli (requires the agent has az cli installed and a service connection is available with write rights on the RG)

TLS/SSL settings : Set https only to true


**3) Create App Service to host documentation-host**
From within your RG click on **+ Create** and select **Web App**
Set a meaningful name for it, in our POC we went for **hugo-docs**
Set Publish : "code"
Set Run time stack : .NET 5
Choose correct region 
Choose correct RG
Choose the App service plan the one created above - **IMPORTANT** make sure you choose the App service plan created in the same RG
After creation : 
Scale out according to environment, but minimum recommended 2 in any environment (to spot in advance issues that shows up in a web farm configuration only). 
Configuration -> General Settings : Set always on to true 
Configuration -> Application settings : Add 

ASPNETCORE_ENVIRONMENT = **\<environmentName\>** : \<env\> can be used, but it's likely better to pick up a more meaningful code for the environment, instead of the single acronym value. 
This value MUST be in sync with the Environment name of the release pipeline AND match the config file names appsettings.**\<environmentName\>**.json.

Setting of ASPNETCORE_ENVIRONMENT is required ONLY if this value is not set during the release pipeline using azure cli (requires the agent has az cli installed and a service connection is available with write rights on the RG)

TLS/SSL settings : Set https only to true

List of resources in the RG

![Resources](/markdown-guidance/azure-resources.png)