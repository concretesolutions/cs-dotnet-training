let testResultModule = require('../logging/testResult.js');

module.exports = {

  deleteAllAsync: (req, res, options) => {

    return new Promise((resolve, reject) => {

      options.testResultAsyncDAO.deleteAllAsync(options.connPool)
        .then((affected) => {
          res.status(200);
          res.body = null;
          resolve();
        })
        .catch(reject);
    });
  },

  postTestResultsFromEventsAsync: (req, res, options) => {

    let eventResultArray = options.eventResultDAO.selectAll();
    let testResultArray = testResultModule.compileTestResult(eventResultArray);

    return new Promise((resolve, reject) => {

      options.testResultAsyncDAO.insertRangeAsync(testResultArray, options.connPool)
        .then(() => {
          res.status(200);
          res.body = null;
          resolve();
        })
        .catch(reject);
    });
  }
}