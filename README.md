# KYC.API.Proxy

## Overview

This repository contains the code for the KYC.API.Proxy AWS Lambda Function.
The function is designed to act as a proxy for KYC (Know Your Customer) operations, making authenticated requests to the KYC API from Blockpass.
It provides a simplified API for consuming services from Blockpass and returns data in a structured JSON format.

## Features

- Securely retrieves secrets for API authentication using [SecretsManager](https://github.com/The-Poolz/SecretsManager).
- Responds with a 403 status code for invalid address input.

## Environment Variables

To run this Lambda function, need to set the following environment variables:

- SECRET_ID: Identifier for the secret to be used for API authentication.
- SECRET_API_KEY: Name of the secret key to be used for API authentication.
- CLIENT_ID: Client ID for the KYC Blockpass API.

## How it Works

The lambda function uses the Flurl.Http library to make an HTTP GET request to the KYC API from Blockpass.
It authenticates the request using a secret API key retrieved from the AWS SecretsManager.
The retrieved data is then returned as a JSON token.
