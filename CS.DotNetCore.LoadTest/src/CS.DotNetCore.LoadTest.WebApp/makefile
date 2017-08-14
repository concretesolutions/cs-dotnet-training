sourceDir=/media/sf_projetos/miscellaneous_net_core/src/CS.DotNetCore.LoadTest.WebApp
webAppDir=/media/sf_projetos/miscellaneous_net_core/deploy/cs.dotnetcore.loadtest.webapp/staging

#copy app and environment files from windows host to linux VM
install:
	mkdir -p ./docker
	rm -rf ./docker/*
	
	cp -rf $(sourceDir)/Docker/* ./docker
	mkdir -p ./webapp
	
	rm -rf ./webapp/*
	cp -rf $(webAppDir)/* ./webapp
	
	chmod -R 755 ./docker
	chmod -R 755 ./webapp