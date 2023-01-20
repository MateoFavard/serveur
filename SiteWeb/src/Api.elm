module Api exposing (
  Error(..),
  Result,
  signUpRequest,
  signInRequest,
  validationRequest  )

import Http
import Json.Encode as Encode
import Json.Decode as Decode
import Base64.Encode as Base64
import Session exposing (User(..))


-- Requests

toAuthHeader : User -> List Http.Header
toAuthHeader user = 
  case user of 
    Visitor ->
      [ ]

    NotValidated basic ->
      [ Http.header "Authorization" (
          "Basic " ++ Base64.encode (
            Base64.string (
              basic.userId ++ ":" ++ basic.password
            )
          )
        )
      ]

    Validated bearer ->
      [ Http.header "Authorization" ("Bearer " ++ bearer.token) ]


request :
  { method : String
  , credentials : User
  , endpoint : String
  , body : Http.Body
  , expect : Http.Expect msg
  }
  -> Cmd msg
request req = 
  Http.request
    { method = req.method
    , headers = toAuthHeader req.credentials
    , url = "/api" ++ req.endpoint 
    , body = req.body
    , expect = req.expect
    , timeout = Nothing
    , tracker = Nothing
    }


-- Expectations

type Error
  = Internal
  | Network
  | Message String
  | Unauthorized


type alias Result a = Result.Result Error a


expectBase :
  (Result a -> msg)
  -> (String -> Result a)
  -> Http.Expect msg
expectBase toMsg fromString =
  Http.expectStringResponse toMsg <|
    \response ->
      case response of
        Http.BadUrl_ url ->
          Err Internal

        Http.Timeout_ ->
          Err Network

        Http.NetworkError_ ->
          Err Network

        Http.BadStatus_ metadata body ->
          case metadata.statusCode of
            400 -> Err (Message body)
            401 -> Err Unauthorized
            _ -> Err Internal

        Http.GoodStatus_ metadata body ->
          fromString body


expectWhatever : (Result () -> msg) -> Http.Expect msg
expectWhatever toMsg =
  expectBase toMsg <|
    \body -> Ok ()


expectJson : Decode.Decoder a -> (Result a -> msg) -> Http.Expect msg
expectJson decoder toMsg =
  expectBase toMsg <|
    \body -> Result.mapError (\err -> Internal) (Decode.decodeString decoder body)


-- Sign up

type alias SignUpPayload =
  { username : String
  , email : String
  , captcha : String
  }


encodeSignUpPayload : SignUpPayload -> Encode.Value
encodeSignUpPayload payload =
  Encode.object
    [ ( "Username", Encode.string payload.username )
    , ( "Email", Encode.string payload.email )
    , ( "CaptchaToken", Encode.string payload.captcha )
    ]


signUpRequest : (Result () -> msg) -> SignUpPayload -> Cmd msg
signUpRequest response payload =
  request
    { method = "POST"
    , endpoint = "/account/signup"
    , credentials = Visitor
    , body = Http.jsonBody (encodeSignUpPayload payload)
    , expect = expectWhatever response
    }


-- Sign In

decodeSignInResponse : Decode.Decoder (Maybe String)
decodeSignInResponse =
  Decode.maybe (Decode.field "SessionToken" Decode.string)


signInRequest : (Result (Maybe String) -> msg) -> User -> Cmd msg
signInRequest response creds =
  request
    { method = "POST"
    , endpoint = "/account/signin"
    , credentials = creds
    , body = Http.emptyBody
    , expect = expectJson decodeSignInResponse response
    }


-- Validation

type alias ValidationPayload =
  { newPassword : String
  }


encodeValidationPayload : ValidationPayload -> Encode.Value
encodeValidationPayload payload =
  Encode.string payload.newPassword


validationRequest : (Result () -> msg) -> User -> ValidationPayload -> Cmd msg
validationRequest response creds payload =
  request
    { method = "POST"
    , endpoint = "/account/validate"
    , credentials = creds
    , body = Http.jsonBody (encodeValidationPayload payload)
    , expect = expectWhatever response
    }