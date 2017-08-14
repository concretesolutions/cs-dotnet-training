module.exports = {
  none: { id: 0, name: 'None' },
  postIdentityMemory: { id: 1, name: 'PostIdentityMemory' },
  postIdentityDb: { id: 2, name: 'PostIdentityDb' },

  getExpectedStatusCode: (eventId) => {

    if (eventId.id == module.exports.postIdentityMemory.id) {
      return 201;
    }

    if (eventId.id == module.exports.postIdentityDb.id) {
      return 201;
    }

    return 200;
  },

  create: (eventId, eventInputs) => {

    let self = {
      eventId: eventId,
      eventInputs: eventInputs
    };

    return self;
  },

  createPostIdentityMemory: (eventInputs) => {
    return module.exports.create(module.exports.postIdentityMemory, eventInputs);
  },

  createPostIdentityDb: (eventInputs) => {
    return module.exports.create(module.exports.postIdentityDb, eventInputs);
  },

  createNone: (eventInputs) => {
    return module.exports.create(module.exports.none, eventInputs);
  }
}