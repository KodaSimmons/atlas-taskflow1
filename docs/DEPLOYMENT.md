# Deploying Atlas TaskFlow to Azure

Atlas runs on **Azure App Service** (the hosting choice from the Sprint 3 architecture). Every push to `main` is built,
tested, deployed and smoke-tested by GitHub Actions (`.github/workflows/azure-deploy.yml`).

## One-time setup

You need an Azure account. [Azure for Students](https://azure.microsoft.com/free/students/) works and needs no card.

### 1. Create the App Service

1. In the [Azure portal](https://portal.azure.com), choose **Create a resource > Web App**.
2. Fill in:
   - **Resource group:** create one, e.g. `atlas-taskflow-rg`. If your school manages the subscription and you can't
     create one, choose **Select existing** and use the group you were given
   - **Name:** something unique, e.g. `atlas-taskflow-koda`. This becomes `https://<name>.azurewebsites.net`
   - **Publish:** Code
   - **Runtime stack:** .NET 8 (LTS)
   - **Operating system:** Linux
   - **Region:** one your subscription allows. Azure for Students only permits some regions; if you get "disallowed by
     Azure", try another (North Central US worked for this project), or check **Policy > Assignments > Allowed
     resource deployment regions** for the list
   - **Pricing plan:** Free F1 is enough for this sprint
3. Click **Review + create**, then **Create**.

> Keep the app at **one instance** (the default). Data is held in memory, so a second instance would have its own
> separate copy.

### 2. Let GitHub deploy to it

1. Open the new Web App. Go to **Settings > Configuration > General settings**, turn **SCM Basic Auth Publishing
   Credentials** **On**, and save. (Azure turns this off by default, and the publish profile needs it.)
2. On the **Overview** page click **Download publish profile**. This is a password for deploying, so don't commit it or
   paste it anywhere public.
3. In the GitHub repo go to **Settings > Secrets and variables > Actions**:
   - **Secrets** tab: **New repository secret**, name `AZURE_WEBAPP_PUBLISH_PROFILE`, value: the whole contents of the
     downloaded file.
   - **Variables** tab: **New repository variable**, name `AZURE_WEBAPP_NAME`, value: your app name (e.g. `atlas-taskflow-koda`).

### 3. Deploy

Push to `main`, or open **Actions > Build, test and deploy to Azure > Run workflow**. The run has two jobs:

- **build**: restore, build, run all tests, publish
- **deploy**: upload to App Service, then run the post-deployment smoke test against the live URL

Until the variable from step 2 exists, the deploy job is skipped and only build and tests run.

## Configuration notes

- No connection strings or secrets are needed yet. The app has no database this sprint.
- App Service sets `ASPNETCORE_ENVIRONMENT=Production` by default, which turns on HSTS and the friendly error page.
- `/health` returns `Healthy` and is used by the smoke test. You can also set it as the App Service health check path
  (**Monitoring > Health check**).
- Sample data resets whenever the app restarts, including after each deployment and when the Free tier idles out.

## Evidence to collect for the assignment

- A screenshot of the green workflow run in the **Actions** tab, with the smoke test step expanded
- The live URL loading in a browser
- One workflow working live (create, submit, approve), see the checklist in [TESTING.md](TESTING.md)
