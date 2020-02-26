Allows storing of Microsoft Orleans grain state in Couchbase.

![Master branch build](https://github.com/mrd1234/Orleans.Persistence.Couchbase/workflows/Master%20branch%20build/badge.svg?branch=master)

Install the package and add the following to your appsettings.json file for the Orleans silo project (changing the values to suit of course):

```javascript
"couchbase": {
    "clientConfiguration": {
      "username": "username",
      "password": "password",
      "servers": [
        "http://some.server:8091/"
      ],
      "buckets": [
        {
          "name": "bucketName"
        }
      ]
    }
  }
