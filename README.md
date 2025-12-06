[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/cqMWIy-z)
[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-2e0aaae1b6195c2367325f4f02e2d04e9abb55f0b24a779b69b11b9e10269abc.svg)](https://classroom.github.com/online_ide?assignment_repo_id=19104332&assignment_repo_type=AssignmentRepo)

You can create a sqlite database file by running the file 'sql/001-init.sql' with the sqlite3 client. 
```
touch app.db
sqlite3 app.db < 001-init.sql
```

### Hosting

Website is available at [zomer.dev](https://zomer.dev)

### Current issues: 

- SQL update trigger not working, updates all rows
- currency value are being read into model as double. May cause rounding errors. May switch to storing currency as either int and dividing by 100, or string and parsing as decimal.

### How to run app:

##### Requirements:
- Docker
- Docker compose

##### Steps:
1. Navigate to project root
2. run the command 'docker compose up --build'


