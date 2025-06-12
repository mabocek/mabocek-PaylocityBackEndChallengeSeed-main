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

- able to view employees and their dependents
- an employee may only have **1 spouse or domestic partner** (not both)  
  - this does not need to be implemented explicitly, but can serve to limit your test cases
- an employee may have an unlimited number of children
- able to calculate and view a paycheck for an employee given the following rules:  
  - 26 paychecks per year with deductions spread as evenly as possible on each paycheck  
  - employees have a base cost of **$1,000 per month** (for benefits)  
  - each dependent represents an additional **$600 cost per month** (for benefits)  
  - employees that make more than **$80,000 per year** will incur an additional **2 % of their yearly salary** in benefits costs  
  - dependents that are **over 50 years old** will incur an additional **$200 per month**  

---

### What are we looking for?

- understanding of business requirements
- correct implementation of requirements
- test coverage for the cost calculations
- code/architecture quality
- plan for future flexibility
- address "task" code comments
- easy to run your code (if non-standard, provide directions)

---

### What should you not waste time on?

- authentication/authorization
- input sanitization
- logging
- adding multiple projects to represent layers… putting everything in the API project is fine