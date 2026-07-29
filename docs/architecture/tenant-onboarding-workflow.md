# Tenant Onboarding Workflow

This document outlines the end-to-end operational workflows for onboarding new tenants to the BaseTemplate SaaS platform. Because the platform supports both **Pooled** and **Isolated** isolation tiers, the onboarding steps vary based on the level of isolation required by the client.

## 1. Onboarding a Pooled Tenant

A pooled tenant shares compute, database, and caching infrastructure with other pooled tenants. Data is logically isolated via row-level security and EF Core Global Query Filters. This is the fastest and cheapest way to onboard a new customer.

### Workflow

1. **No Infrastructure Changes Needed:** 
   Because pooled tenants share infrastructure, you do not need to run any Terraform or Bicep pipelines. The shared resources (e.g., `default-pooled-stamp`) already exist.
   
2. **Register the Tenant (Control Panel):**
   - An administrator logs into the BaseTemplate Control Panel.
   - Navigates to **Tenants** -> **Add Tenant**.
   - Fills in the tenant details (e.g., Name, Domain).
   - Selects the existing **Pooled Deployment Stamp** (e.g., `default-pooled-stamp`).
   - Saves the tenant.

3. **Provisioning Complete:**
   - The system creates the tenant record in the Control Plane database.
   - The tenant is immediately active. When users for this tenant log in, the application routes their traffic to the shared infrastructure and uses their `TenantId` to filter data automatically.

---

## 2. Onboarding an Isolated Tenant

An isolated tenant receives their own dedicated physical infrastructure. They do not share compute, databases, or key vaults with any other tenant. This tier requires provisioning new Azure resources before the tenant can be registered in the software.

### Workflow

1. **Provision Infrastructure (IaC Pipeline):**
   - The infrastructure team (or automated CI/CD pipeline) runs the Bicep/Terraform scripts to provision a new **Deployment Stamp** for the client.
   - This provisions a new Azure Resource Group, App Service / Container App, SQL Database, Redis Cache, and a dedicated **Azure Key Vault**.
   - The pipeline securely saves the connection strings (Database, Cache, etc.) into the newly provisioned Azure Key Vault using our standard convention (e.g., `ConnectionStrings--Database`).

2. **Retrieve the Key Vault URI:**
   - Once the infrastructure pipeline completes, it will output the URI of the newly created Key Vault (e.g., `https://kv-clientxyz-prod.vault.azure.net/`).

3. **Register the Deployment Stamp (Control Panel):**
   - An administrator logs into the BaseTemplate Control Panel.
   - Navigates to **Deployment Stamps** -> **Add Stamp**.
   - Creates a new stamp record representing the infrastructure provisioned in Step 1.
   - Sets the Tier to **Isolated**.
   - Inputs the **Key Vault URI** obtained in Step 2. (The application will use its Managed Identity to read the actual connection strings from this vault at runtime).

4. **Register the Tenant (Control Panel):**
   - The administrator navigates to **Tenants** -> **Add Tenant**.
   - Fills in the tenant details (e.g., Name, Domain).
   - Links the tenant to the **newly created Isolated Deployment Stamp** from Step 3.
   - Saves the tenant.

5. **Provisioning Complete:**
   - The system creates the tenant record in the Control Plane.
   - When traffic arrives for this tenant, the Control Plane resolves the Isolated Stamp.
   - The ASP.NET Core backend dynamically reads the connection strings from the tenant's isolated Key Vault and establishes a connection to their dedicated database.

---

## Summary of Differences

| Phase | Pooled Tenant | Isolated Tenant |
| :--- | :--- | :--- |
| **Infrastructure** | Uses existing shared resources. | Requires running IaC pipelines to provision dedicated resources (App, DB, Key Vault). |
| **Deployment Stamp** | Reuses an existing `Pooled` stamp. | Requires creating a new `Isolated` stamp in the Control Panel using the new Key Vault URI. |
| **Cost & Time** | Instant onboarding, lowest cost. | Slower onboarding (infrastructure deployment time), premium cost. |
| **Data Isolation** | Logical (Row-Level Security, EF Filters). | Physical (Dedicated Database). |
