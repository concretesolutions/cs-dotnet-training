let identity = require('../business/identity.js');
let event = require('../logging/event.js');

module.exports = {

  postIdentityMemory: (req, res, inj) => {

    req.eventContext.event = event.createPostIdentityMemory([req.body]);
    let objIdentity = identity.create(req.body.identityName);

    objIdentity.setPassword(req.body.password, inj.config);
    inj.identityDAO.insert(objIdentity);

    res.status(201);
    res.body = null;
  },

  postIdentityDbAsync: (req, res, inj) => {

    req.eventContext.event = event.createPostIdentityDb([req.body]);
    let objIdentity = identity.create(req.body.identityName);

    objIdentity.setPassword(req.body.password, inj.config);

    return new Promise((resolve, reject) => {

      inj.identityAsyncDAO.insertAsync(objIdentity, inj.connPool)
        .then(() => {
          res.status(201);
          res.body = null;
          resolve();
        })
        .catch(reject);
    });
  },

  deleteAllIdentitiesMemory: (req, res, inj) => {
    inj.identityDAO.deleteAll();
    res.status(200);
    res.body = null;
  },

  deleteAllIdentitiesDbAsync: (req, res, inj) => {

    return new Promise((resolve, reject) => {

      inj.identityAsyncDAO.deleteAllAsync(inj.connPool)
        .then((affected) => {
          res.status(200);
          res.body = null;
          resolve();
        })
        .catch(reject);
    });
  }
};