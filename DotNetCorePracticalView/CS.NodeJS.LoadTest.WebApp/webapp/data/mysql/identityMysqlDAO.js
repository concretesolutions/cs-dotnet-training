let schema = require('../loadTestSchema.js')
let mysql = require('mysql');

module.exports = {

  _deleteAllSqlCommand: 'delete from `' + schema.identity.table + '`;',

  _insertSqlCommand: 'insert into `' + schema.identity.table + '` (`' + schema.identity.identityNameColumn +
  '`,`' + schema.identity.dataColumn + '`) values (?, ?);',

  insertAsync: (identity, connPool) => {

    let cmdParams = [identity.identityName, JSON.stringify(identity)];
    let sql = mysql.format(module.exports._insertSqlCommand, cmdParams);

    return new Promise((res, rej) => {

      connPool.query(sql, (err, result) => {

        if (err) {
          rej(err);
        }
        else {
          res();
        }
      });
    });
  },

  deleteAllAsync: (connPool) => {

    return new Promise((res, rej) => {

      connPool.query(module.exports._deleteAllSqlCommand, (err, result) => {

        if (err) {
          rej(err);
        }
        else {
          res(result.affectedRows);
        }
      });
    });
  }
}