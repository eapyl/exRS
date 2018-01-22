# exRS
Tool to deploy SSRS reports and subscriptions

exRSConsole - console application to run deployment:
* exRSConsole.exe -n Report1 -p SampleReport -r -s

where
* -n Report1 - the name of report which will be deployed
* -p SampleReport - the folder at SSRS server where the report will be created
* -r - means to deploy report
* -s - meams to deploy subscription

It is needed to create folder SampleReport with Report1.rdl (and DataSet1.rsd, DataSource1.rds) files in Reports folder near exRSConsole.exe to run a tool and create a report.
See exRSConsole project as example.

Is is needed to create Report1.xml in Subscriptions folder near exRSConsole.exe to run a tool and create a subscription for Report1.
See exRSConsole project as example.

*Another mods*:

1. exRSConsole.exe -b
> backup all exisiting reports and subscriptions to folder which are in app.congif (backupPath setting)
2. exRSConsole.exe -d
> delete all folders at SSRS server

To run a tool need to set up the next settings in configuration file:
```
  <applicationSettings>
    <exRS.Properties.Settings>
      <!-- URI of SSRS server -->
      <setting name="exRS_SSRSService_ReportingService2010" serializeAs="String">
        <value>http://desktop-name:80/ReportServer/ReportService2010.asmx</value>
      </setting>
    </exRS.Properties.Settings>
  </applicationSettings>
  <userSettings>
    <exRS.Properties.Settings>
      <!-- a user to connect to SSRS server -->
      <setting name="SSRSUser" serializeAs="String">
        <value>desktop-name\user</value>
      </setting>
      <!-- a password to connect to SSRS server -->
      <setting name="SSRSPassword" serializeAs="String">
        <value></value>
      </setting>
      <!-- a path to folder where put reports and subscriptions during backup process -->
      <setting name="backupPath" serializeAs="String">
        <value>C:\Temp</value>
      </setting>
      <!-- a name of folder with subscription configuration for reports (near exRSConsole.exe) -->
      <setting name="subscriptionSettingsFolder" serializeAs="String">
        <value>Subscriptions</value>
      </setting>
      <!-- a user name to connect to DB for data sources -->
      <setting name="dbUserForDataSource" serializeAs="String">
        <value>test</value>
      </setting>
      <!-- a password name to connect to DB for data sources -->
      <setting name="dbPasswordForDataSource" serializeAs="String">
        <value>test</value>
      </setting>
      <!-- a path at SSRS server where put reports created by subscription -->
      <setting name="subscriptionFileSharePath" serializeAs="String">
        <value>\\DESKTOP-NAME\Temp</value>
      </setting>
      <!-- a name of folder with reports (*.rdl, *rds and *.rsd files) (near exRSConsole.exe) -->
      <setting name="sourceFolderPath" serializeAs="String">
        <value>.\Reports</value>
      </setting>
      <!-- a name of folder at SSRS server with data sources -->
      <setting name="datasourcesServerFolderName" serializeAs="String">
        <value>Data Sources</value>
      </setting>
      <!-- a name of folder at SSRS server with datasets-->
      <setting name="datasetsServerFolderName" serializeAs="String">
        <value>Datasets</value>
      </setting>
    </exRS.Properties.Settings>
  </userSettings>
```
