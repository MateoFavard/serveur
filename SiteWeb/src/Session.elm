module Session exposing (Session, User(..), validated, notValidated, visitor)

import Browser.Navigation as Nav

import Route exposing (Route)

type alias Session =
  { key : Nav.Key
  , route : Route
  , user : User
  }


type alias BasicCredentials =
  { userId : String
  , password : String
  }


type alias BearerCredentials =
  { token : String
  }


type User
  = Validated BearerCredentials
  | NotValidated BasicCredentials
  | Visitor


validated : Session -> BearerCredentials -> Session
validated session creds =
  { session | user = Validated creds }


notValidated : Session -> BasicCredentials -> Session
notValidated session creds =
  { session | user = NotValidated creds }


visitor : Session -> Session
visitor session =
  { session | user = Visitor }