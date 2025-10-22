@vaxhub @inventory @proxy
Feature: VaxHub Inventory Product API with Proxy
  As an API consumer
  I want to retrieve inventory product data using VaxHub API with proxy
  So that I can securely access product information through a proxy server

  @vaxhub @inventory @proxy @success
  Scenario: Get inventory product data with VaxHub headers and proxy
    Given the VaxHub API base URL is "https://vhapistg.vaxcare.com"
    And I have proxy configured for "localhost" on port 8888
    And I have VaxHub mobile headers configured for inventory API
    And I have the following request headers:
      | HeaderName       | HeaderValue                                                                 |
      | IsCalledByJob    | true                                                                       |
      | X-VaxHub-Identifier | eyJhbmRyb2lkU2RrIjoyOSwiYW5kcm9pZFZlcnNpb24iOiIxMCIsImFzc2V0VGFnIjotMSwiY2xpbmljSWQiOjg5NTM0LCJkZXZpY2VTZXJpYWxOdW1iZXIiOiJOT19QRVJNSVNTSU9OIiwicGFydG5lcklkIjoxNzg3NjQsInVzZXJJZCI6MCwidXNlck5hbWUiOiAiIiwidmVyc2lvbiI6MTQsInZlcnNpb25OYW1lIjoiMy4wLjAtMC1TVEciLCJtb2RlbFR5cGUiOiJNb2JpbGVIdWIifQ== |
      | traceparent      | 00-3140053e06f8472dbe84f9feafcdb447-55674bbd17d441fe-01                   |
      | MobileData       | false                                                                      |
      | UserSessionId    | NO USER LOGGED IN                                                          |
      | MessageSource    | VaxMobile                                                                  |
      | Host             | vhapistg.vaxcare.com                                                       |
      | Connection       | Keep-Alive                                                                 |
      | User-Agent       | okhttp/4.12.0                                                              |
    When I send a GET request to "/api/inventory/product/v2"
    Then the VaxHub response status should be 200 OK
    And the VaxHub response time should be less than 10 seconds
    And the response should contain "Content-Type" header with "application/json"
    And the response should contain inventory product data
