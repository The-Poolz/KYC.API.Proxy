# KYC.API.Proxy

## Overview

This repository contains the code for the KYC.API.Proxy AWS Lambda Function.
The function is designed to act as a proxy for KYC (Know Your Customer) operations, making authenticated requests to the KYC API from Blockpass.
It provides a simplified API for consuming services from Blockpass and returns data in a structured JSON format.

## Features

- Securely retrieves secrets for API authentication using [SecretsManager](https://github.com/The-Poolz/SecretsManager).

## Environment Variables

To run this Lambda function, need to set the following environment variables:

- SECRET_ID: Identifier for the secret to be used for API authentication.
- SECRET_API_KEY: Name of the secret key to be used for API authentication.
- CLIENT_ID: Client ID for the KYC Blockpass API.

## How it Works

The lambda function uses the Flurl.Http library to make an HTTP GET request to the KYC API from Blockpass.
It authenticates the request using a secret API key retrieved from the AWS SecretsManager.
The retrieved data is then returned as a JSON token.

## Responses in API4

- User authorized in Blockpass

```json
{
    "data": {
        "myProxyKYC": {
            "RequestStatus": "success",
            "Status": "approved",
            "Name": "NAME OF USER"
        }
    }
}
```

- User not authorized in Blockpass

```json
{
    "data": {
        "myProxyKYC": {
            "RequestStatus": "error",
            "Status": null,
            "Name": null
        }
    }
}
```

- Proxy response (via linked address)

```json
{
    "data": {
        "myProxyKYC": {
            "RequestStatus": "success",
            "Status": "approved",
            "Name": "NAME OF USER"
            "Proxy": "0xlinked address"
        }
    }
}
```

- Other Status

 ```json
{
    "data": {
        "myProxyKYC": {
            "RequestStatus": "success",
            "Status": "not finish",
            "Name": null
            "Proxy": "0xlinked address"
        }
    }
}
```
