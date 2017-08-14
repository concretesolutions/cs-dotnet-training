sourceDir=/media/sf_projetos/miscellaneous_nodejs/CS.NodeJS.LoadTest.WebApp

install:
	mkdir -p ./docker
	rm -rf ./docker/*
	
	cp -rf $(sourceDir)/docker/* ./docker
	mkdir -p ./webapp
	
	rm -rf ./webapp/*
	cp -rf $(sourceDir)/webapp/* ./webapp
	
	chmod -R 755 ./docker
	chmod -R 755 ./webapp