# C200-BatchAuditProcessing


## Recommended IDEs
VS Community 2017
VS Code

## Source Organization
### CTCBatchAuditProcessing 
Visual Studio Solution - library and conole application projects.

#### \CTCBAParser
.NET class library for code to process SBCTC degree audit batch-audit output. 

#### \CTCBAStorage
.NET class library. Code specific to persistance of results from the CTCBAParser library to database storage. Uses Linq to SQL. 

#### \CTCBatchAuditProcessor
.NET console application project. Used to process batch audit results from the windows command line/task scheduler.

#### \SD4006SFileGenerator
.net console application project. Used to generate file format required by the Legacy SD4006 process.

#### \TestConsole
.NET console application project. Used for basic integration testing or any other non-production needs.

### SBCTC-DOC
Copies of official SBCTC documentation around the Degree Audit product.
see under Student Progress: https://www.sbctc.edu/colleges-staff/it-support/legacy-applications/sms/module-documentation.aspx 


