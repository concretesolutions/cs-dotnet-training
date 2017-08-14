let schema = require('./loadTestMongoSchema.js')

module.exports = {

  _insertOneOptions: { bypassDocumentValidation: false },

  insertAsync: (identity, pool) => {

    return new Promise((res, rej) => {

      pool.getDbAsync()
        .then((db) => {

          db.collection(schema.identity.collection)
            .insertOne(identity, module.exports._insertOneOptions, (err, result) => {

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

  deleteAllAsync: (pool) => {

    return new Promise((res, rej) => {

      pool.getDbAsync()
        .then((db) => {

          db.collection(schema.identity.collection)
            .deleteMany({}, (err, result) => {

              if (err) {
                rej(err);
              }
              else {
                res(result.result.n);
              }
            });
        })
        .catch(rej);
    });
  }
}