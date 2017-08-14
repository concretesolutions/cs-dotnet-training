let port = 5051;
let pg = require('pg');

let express = require('express');
let bodyParser = require('body-parser');

let appSettings = require('./config/appsettings.js');
let connectionPool = require('./data/connectionPool.js');

let dataMigratioDAO = require('./data/dataMigrationDAO.js');
let dataMigration = require('./data/dataMigration.js');

let eventResultMiddleware = require('./middlewares/eventResutMiddleware.js');
let loadtestMiddleware = require('./middlewares/loadtestMiddleware.js');

let identityController = require('./controllers/identityController.js');
let testController = require('./controllers/testController.js');
let eventResultController = require('./controllers/eventResultController.js');

let identityDAO = require('./data/memory/identityMemoryDAO.js');
let identityAsyncDAO = require('./data/identityAsyncDAO.js');

let eventResultDAO = require('./data/memory/eventResultMemoryDAO.js');
let testResultAsyncDAO = require('./data/testResultAsyncDAO.js');

let buildIdentityRoutes = (api, connPool) => {

  api.post('/api/v1/identity/memory', (req, res, next) => {

    try {

      identityController.postIdentityMemory(req, res, {
        identityDAO: identityDAO,
        config: appSettings
      });

      next();
    }
    catch (err) {
      next(err);
    }
  });

  api.post('/api/v1/identity/db', (req, res, next) => {

    try {

      identityController.postIdentityDbAsync(req, res, {
        identityAsyncDAO: identityAsyncDAO,
        config: appSettings,
        connPool: connPool
      })
        .then(next)
        .catch(next);
    }
    catch (err) {
      next(err);
    }
  });

  api.delete('/api/v1/identity/all/memory', (req, res, next) => {

    try {

      identityController.deleteAllIdentitiesMemory(req, res, {
        identityDAO: identityDAO
      });

      next();
    }
    catch (err) {
      next(err);
    }
  });

  api.delete('/api/v1/identity/all/db', (req, res, next) => {

    try {

      identityController.deleteAllIdentitiesDbAsync(req, res, {
        identityAsyncDAO: identityAsyncDAO,
        connPool: connPool
      })
        .then(next)
        .catch(next);
    }
    catch (err) {
      next(err);
    }
  });
};

let buildTestRoutes = (api, connPool) => {

  api.delete('/api/v1/test/all', (req, res, next) => {

    try {

      testController.deleteAllAsync(req, res, {
        testResultAsyncDAO: testResultAsyncDAO,
        connPool: connPool
      })
        .then(next)
        .catch(next);
    }
    catch (err) {
      next(err);
    }
  });

  api.post('/api/v1/test/all/event_result', (req, res, next) => {

    try {
      testController.postTestResultsFromEventsAsync(req, res, {
        eventResultDAO: eventResultDAO,
        testResultAsyncDAO: testResultAsyncDAO,
        connPool: connPool
      })
        .then(next)
        .catch(next);
    }
    catch (err) {
      next(err);
    }

  })
};

let buildEventResultRoutes = (api) => {

  api.delete('/api/v1/event_result/all', (req, res, next) => {

    try {

      eventResultController.deleteAllFromMemory(req, res, {
        eventResultDAO: eventResultDAO
      });

      next();
    }
    catch (err) {
      next(err);
    }
  });
};

let buildAppPipeline = (connPool) => {

  //building app pipeline
  let eventResultMiddlewareObj = eventResultMiddleware.create({
    eventResultDAO: eventResultDAO,
  });

  let api = express();
  api.use(eventResultMiddleware.initEventContext);

  api.use(bodyParser.json());
  api.use(loadtestMiddleware.handle404);

  buildIdentityRoutes(api, connPool);
  buildTestRoutes(api, connPool);
  buildEventResultRoutes(api);

  api.use(loadtestMiddleware.handleError);
  api.use(eventResultMiddlewareObj.saveEventResult);
  api.use(loadtestMiddleware.sendResponse);

  api.listen(port);
  console.log('listening(port:' + port + ')...');
};

let migrationErrorHandler = (err) => {
  console.log('migration fail.')
  console.log(err);
  process.exit(2);
};

dataMigratioDAO.createAsync({
  appSettings: appSettings
})
  .then((dao) => {

    dataMigration.executeAsync({
      migrationDAO: dao
    })
      .then(() => {

        dao.disposeAsync()
          .then(() => {

            connectionPool.createAsync({ appSettings: appSettings })
              .then((pool) => {

                buildAppPipeline(pool);
              })
              .catch(err => {
                console.log('start app fail.');
                console.log(err)
                process.exit(1);
              });
          })
          .catch(migrationErrorHandler);
      })
      .catch(migrationErrorHandler)
  })
  .catch(migrationErrorHandler);