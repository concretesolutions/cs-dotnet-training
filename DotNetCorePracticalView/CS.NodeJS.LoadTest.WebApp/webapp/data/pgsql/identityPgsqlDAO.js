let schema = require('../loadTestSchema.js')

module.exports = {

  _deleteAllSqlCommand: 'delete from "' + schema.identity.table + '";',

  _insertSqlCommand: 'insert into "' + schema.identity.table + '" ("' + schema.identity.identityNameColumn +
  '","' + schema.identity.dataColumn + '") values ($1, $2);',

  insertAsync: (identity, connPool) => {

    let queryConfig = {
      text: module.exports._insertSqlCommand,
      values: [identity.identityName, identity]
    };

    return new Promise((res, rej) => {

      connPool.connect()
        .then(cl => {

          let insert = cl.query(queryConfig, (err, result) => {

            cl.release();

            if (err) {
              rej(err);
            }
            else {
              res();
            }
          });
        })
        .catch(rej);
    });
  },

  deleteAllAsync: (connPool) => {

    let queryConfig = {
      text: module.exports._deleteAllSqlCommand,
      values: []
    };

    return new Promise((resolve, reject) => {

      connPool.connect()
        .then(cl => {

          let deleteCmd = cl.query(queryConfig, (err, result) => {

            cl.release();

            if (err) {
              reject(err);
            }
            else {
              resolve();
            }
          });
        })
        .catch(reject)
    });
  }
}