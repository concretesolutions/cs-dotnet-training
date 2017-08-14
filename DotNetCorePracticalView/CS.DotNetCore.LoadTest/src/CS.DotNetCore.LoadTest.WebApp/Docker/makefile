refreshPgsql:
	rm -rf ../../database/pgsql/data/*

refreshMongo:
	rm -rf ../../database/mongo/data/*

refreshMysql:
	rm -rf ../../database/mysql/data/*

copyApp:
	mkdir -p webapp
	rm -rf ./webapp/*
	cp -rf ../webapp/* ./webapp
	

startAppPgsql: copyApp
	docker-compose --file docker-compose-pgsql.yml up --build --force-recreate

startAppMysql: copyApp
	docker-compose --file docker-compose-mysql.yml up --build --force-recreate

startAppMongodb: copyApp
	docker-compose --file docker-compose-mongodb.yml up --build --force-recreate
