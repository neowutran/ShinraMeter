WIP - Just a test
=====
- Datastorage refactoring
- Switch to sqlite for datastorage instead of: storing everything in class & reimplementing custom select, delete, insert etc... 
Sqlite database stored in memory, no percistent file


With that I except
======
no more deep clone to send data to the UI. Instead use the "select" of sqlite. => decrease CPU usage?
Less custom code => standard sql query, easier to understand

TODO
=====
- Database schema
- ...

