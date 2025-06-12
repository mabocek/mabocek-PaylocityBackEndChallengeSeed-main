# Instructions for Coding Assessment:


**How do I get started?**
Create new GitHub repository, seeded from provided zip file
Implement requirements
Document any decisions that you make with comments explaining "why"
Provide us with a link to your code repository

**Requirements**

**Instructions for Coding Assessment**

### How do I get started?

a. Create new GitHub repository, seeded from provided zip file  
b. Implement requirements  
c. Document any decisions that you make with comments explaining "why"  
d. Provide us with a link to your code repository  

---

### Requirements

#### Able to view employees and their dependents - ####
- ✅ Employee endpoints: `GET /api/v1/employees` and `GET /api/v1/employees/{id}`
- ✅ Dependent endpoints: `GET /api/v1/dependents` and `GET /api/v1/dependents/{id}`

####  An employee may only have **1 spouse or domestic partner** (not both)  ####
- ✅ Enforced ValidateSpouseAndDomesticPartnerConstraint method in CreateEmployeeWithDependentsCommandHandler. Can be also enforced through filtered index in database, but it's not compatible with In-Memory databae, which I used in this project.
####  This does not need to be implemented explicitly, but can serve to limit your test cases ####


####  an employee may have an unlimited number of children ####
- ✅ Done and tested with test `ComplexScenario_StandardEmployeeWithMultipleDependents()`
#### able to calculate and view a paycheck for an employee given the following rules:  
- ✅ Done and tested
#### - 26 paychecks per year with deductions spread as evenly as possible on each paycheck  
- ✅ Done and tested with test `CalculateGrossPayPerPaycheck_ShouldDivideBy26Paychecks()`
#### - employees have a base cost of **$1,000 per month** (for benefits)  
- ✅ Done and tested with tests. For example: `ComplexScenario_StandardEmployeeWithMultipleDependents()`
#### - each dependent represents an additional **$600 cost per month** (for benefits)  
- ✅ Done and tested with tests. For example: `CalculateDependentCost_VariousAges_ShouldCalculateCorrectly()`
#### - employees that make more than **$80,000 per year** will incur an additional **2 % of their yearly salary** in benefits costs  
- ✅ Done and tested with tests. For example: `CalculateHighSalaryAdditionalCost_Over80K_ShouldCalculate2Percent()`
#### - dependents that are **over 50 years old** will incur an additional **$200 per month**  
- ✅ Done and tested with tests. For example: `GetPaycheck_ValidEmployee_ShouldReturnCorrectStructure()`

---

### What are we looking for?

#### Understanding of business requirements

##### ✅ Summary

1. **Given** an employee with any salary.  
   **When** benefits are calculated.  
   **Then** the employee incurs $1,000 per month independent of other factors.  

2. **Given** dependents (spouse, partner, or child).  
   **When** benefits are calculated.  
   **Then** $600 is added for each dependent.  

3. **Given** an employee whose salary exceeds $80,000.  
   **When** benefits are calculated.  
   **Then** 2% of the annual salary divided by 12 is added.  

4. **Given** a dependent whose age is greater than 50 years.  
   **When** benefits are calculated.  
   **Then** $200 is added per such dependent.  

5. **Given** total monthly benefit cost & annual salary.  
   **When** payroll is processed 26 times a year.  
   **Then** gross pay, deductions, and net pay are each divided by 26.


##### ✅ Basic test cases with provided seed data

**LeBron James (Employee ID: 1)**  
- **Salary:** $75,420.99 (under $80K threshold)  
- **Dependents:** None  
- **Monthly Costs:** $1,000 (employee base only)  
- **Gross Pay per Paycheck:** $2,900.81  
- **Benefit Deductions per Paycheck:** $461.54  
- **Net Pay per Paycheck:** $2,439.27  

---

**Ja Morant (Employee ID: 2)**  
- **Salary:** $92,365.22 (over $80K threshold)  
- **Dependents:** 3 (spouse + 2 children, all under 50)  
- **Monthly Costs:**  
  - Employee base: $1,000  
  - Dependents: $1,800 (3 × $600)  
  - High salary: $153.94 (2% × $92,365.22 ÷ 12)  
  - **Total:** $2,953.94 per month  
- **Gross Pay per Paycheck:** $3,552.51  
- **Benefit Deductions per Paycheck:** $1,363.36  
- **Net Pay per Paycheck:** $2,189.15  

---

**Michael Jordan (Employee ID: 3)**  
- **Salary:** $143,211.12 (over $80K threshold)  
- **Dependents:** 1 (domestic partner, over 50)  
- **Monthly Costs:**  
  - Employee base: $1,000  
  - Dependent base: $600  
  - Over-50 additional: $200  
  - High salary: $238.69 (2% × $143,211.12 ÷ 12)  
  - **Total:** $2,038.69 per month  
- **Gross Pay per Paycheck:** $5,508.12  
- **Benefit Deductions per Paycheck:** $941.79  
- **Net Pay per Paycheck:** $4,566.33


####  correct implementation of requirements ####
####  test coverage for the cost calculations ####
- ✅ Cost calculations are covered.
- ⚠️ Further code coverage can be done for CQRS Command and Queriers, Feature Flags and Repositories.
####  code/architecture quality ####
##### ✅ Architecture decisions

**✅ Goals**  
- Safer releases
- Leaner, faster endpoints
- Scalable, test friendly architecture
- Lightweight development & CI/CD setup
- Cleaner domain data
- Decoupled layers

---

**✅ Feature Flags (Toggles)**  
To better control feature releases, Feature Flags are an excellent solution.  
In this implementation, I did not use an external service like FlagSmith—which typically enables decoupled deployment and release—because I wanted to avoid adding a new dependency.  
Instead, Feature Flags are configured via application settings, which can be overridden by environment variables during deployment.  
This provides a basic level of flexibility and enables kill-switch-like behavior.

---

**✅ Minimal API Endpoints**  
Endpoints were migrated to Minimal APIs for their simplicity, performance, and lightweight structure.  
This also helps avoid the "fat controller" anti-pattern.

---

**✅ Service Layer**  
A dedicated service layer handles most of the business logic, keeping endpoints thin and maintainable.

---

**✅ CQRS (using MediatR)**  
CQRS is an excellent pattern for separating responsibilities and improving scalability.  
In high-throughput systems, write operations can be directed to a master node, while read operations are handled by replicas.  
This architecture also supports writing to a normalized relational database while reading from a denormalized structure for better performance.  
MediatR is used to implement this pattern.

---

**✅ API Versioning**  
API project is using `Asp.Versioning.Mvc` for standard versioning.

---

**✅ Repository Pattern**  
The Repository pattern introduces an abstraction over data access, making it easier to switch the database provider and to unit test business logic.

---

**✅ Entity Framework (In-Memory for Temporary Persistence)**  
Entity Framework is used to persist data in an In-Memory database for simplicity and to avoid adding more dependencies.  
Unfortunately, In-Memory databases do not support transactions.  
As a result, the current implementation of the Unit of Work pattern is limited in usefulness.

---

**✅ Unit of Work Pattern**  
This pattern is useful for ensuring data consistency in batch or multi-step operations by committing transactions only after all operations succeed.  
If one fails, the transaction is rolled back to prevent inconsistent data.  
However, with In-Memory EF, transaction support is not available, rendering this implementation mostly ineffective.

---

**✅ Date Handling**  
Previously, dates of birth were stored as `DateTime`, which includes unnecessary time data.  
I changed it to `DateOnly`, which is semantically more accurate and simplifies date-related logic. If it would be needed to calculate exact dates, `DateTimeOffset` would be probably better solution.

---



#### Plan for future flexibility ####

**✅ Entity Framework (Persistent SQL Storage)**  
Entity Framework should be configured to use a real SQL-based persistent store, not just an in-memory one.  
This would enable the full benefit of the Unit of Work pattern and ensure data durability.

---

**✅ Containerization**  
The application should be containerized using Docker, with support for orchestration via Kubernetes and Helm charts.  
This would allow for easier deployment, scaling, and environment parity.

---

**✅ Input Validation**  
Although deprioritized during the current phase, proper input sanitization and data validation are necessary for any production-ready system.

---

**✅ Rate limiting**  
In order to limit at least some attacks on server, rate limiting is needed to elimiate such a threats.

---

**✅ Caching**  
Response/output caching middleware plus Redis/MemoryCache.

---

**✅ Error handling**  
Global exception handler to be implemented.

---

**✅ Observability**  
OpenTelemetry tracing/metrics; correlate requestId through logs.

---

**✅ CI/CD pipeline**  
Solution requires modification of CI/CD pipelines.
- `dotnet.yml`: Currently provides only basic build and unit tests execution.
- `dotnet-with-tests.yml`: (Manually executed now, but should be triggered automatically) Provides build and unit tests execution + integration tests (using WebApplicationFactory) execution. It also requires SAST system to be connected and deployment to servers.
***Contract tests***
In order to avoid common issues with releasing new version and breaking consumer logic, I highly recommend to implement at least consumer Contract tests using Pact.

---

**✅ Layered Project Structure**  
Further separation of concerns can be achieved by splitting the codebase into dedicated projects, such as:  
- `Api.Common`: Shared DTOs, domain models, enums, and interfaces  
- `Api.Data`: Base data access logic  
- `Api.Data.SqlServer`: SQL-specific persistence implementations  
- `Api.Orchestration`: Business logic and orchestration between layers  
- `Api.Output`: API endpoint logic and response shaping  
- `Api.Tests`: Unit and integration test projects  

This modular architecture supports better maintainability, testing, and future scalability..

---

**✅ Contract tests**  
- `dotnet.yml`: Currently provides only basic build and unit tests execution.
- `dotnet-with-tests.yml`: (Manually executed now, but should be triggered automatically) Provides build and unit tests execution + integration tests (using WebApplicationFactory) execution. It also requires SAST system to be connected and deployment to servers.

---

**⚠️ ApiResponse<T>**  
Project is using Generic class for responses. I'm not exactly sure, why it's there and what it really solves, which cannot be done using standard responses, but I kept it. If there is no legimite reason of keeping it, I would remove it.

####  Address "task" code comments ####
####  easy to run your code (if non-standard, provide directions) ####
✅ I tried to make code as standard as possible, no additional commands are needed.

---

### What should you not waste time on?

- ✅ authentication/authorization
- ✅ input sanitization
- ✅ logging
- ✅ adding multiple projects to represent layers… putting everything in the API project is fine


---
# Original ReadMe
---

# What is this?

A project seed for a C# dotnet API ("PaylocityBenefitsCalculator").  It is meant to get you started on the Paylocity BackEnd Coding Challenge by taking some initial setup decisions away.

The goal is to respect your time, avoid live coding, and get a sense for how you work.

# Coding Challenge

**Show us how you work.**

Each of our Paylocity product teams operates like a small startup, empowered to deliver business value in
whatever way they see fit. Because our teams are close knit and fast moving it is imperative that you are able
to work collaboratively with your fellow developers. 

This coding challenge is designed to allow you to demonstrate your abilities and discuss your approach to
design and implementation with your potential colleagues. You are free to use whatever technologies you
prefer but please be prepared to discuss the choices you’ve made. We encourage you to focus on creating a
logical and functional solution rather than one that is completely polished and ready for production.

The challenge can be used as a canvas to capture your strengths in addition to reflecting your overall coding
standards and approach. There’s no right or wrong answer.  It’s more about how you think through the
problem. We’re looking to see your skills in all three tiers so the solution can be used as a conversation piece
to show our teams your abilities across the board.

Requirements will be given separately.