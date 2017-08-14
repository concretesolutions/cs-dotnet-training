let crypto = require('crypto');

module.exports = {

  create: (identityName) => {

    let self = {

      identityName: identityName,
      salt: null,
      password: null
    };

    self.setPassword = (password, config) => {

      const outputEncoding = 'base64',
        cipherAlg = 'aes256',
        hashAlg = 'sha256';

      let saltIV = Buffer.from(config.security.passwordSaltIV, outputEncoding);
      let saltKey = Buffer.from(config.security.passwordSaltKey, outputEncoding);

      let aesCipher = crypto.createCipheriv(cipherAlg, saltKey, saltIV);
      aesCipher.update(config.security.passwordSalt, config.encoding);
      self.salt = aesCipher.final(outputEncoding);

      let shaAlg = crypto.createHash(hashAlg);
      shaAlg.update(config.security.passwordSalt + password, config.encoding);
      self.password = shaAlg.digest(outputEncoding);
    }

    return self;
  }
}