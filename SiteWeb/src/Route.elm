module Route exposing (
  Route (..),
  SignUpFragment (..),
  ValidationFragment (..),
  MyAccountFragment (..),
  fromUrl)

import Url exposing (Url)
import Url.Parser exposing (Parser, parse, (</>), fragment, map, oneOf, s, top)

type Route
  = Home
  | SignIn
  | SignUp SignUpFragment
  | Validation ValidationFragment
  | MyAccount MyAccountFragment
  | NotFound


type SignUpFragment
  = JustSignUp
  | SignUpSuccess


type ValidationFragment
  = JustValidation
  | ValidationSuccess


type MyAccountFragment
  = JustMyAccount
  | SetEmail
  | SetPassword


fromUrl : Url -> Route
fromUrl url = 
  Maybe.withDefault NotFound (parse routeParser url)


routeParser : Parser (Route -> a) a
routeParser =
  oneOf
    [ map Home       top
    , map SignIn     (s "signin")
    , map SignUp     (s "signup" </> fragment signUpFragment)
    , map Validation (s "validation" </> fragment validationFragment)
    , map MyAccount  (s "myaccount" </>  fragment myAccountFragment)
    ]


signUpFragment : Maybe String -> SignUpFragment
signUpFragment fragment =
  case fragment of
    Just "success" -> SignUpSuccess
    _ -> JustSignUp


validationFragment : Maybe String -> ValidationFragment
validationFragment fragment =
  case fragment of
    Just "success" -> ValidationSuccess
    _ -> JustValidation


myAccountFragment : Maybe String -> MyAccountFragment
myAccountFragment fragment =
  case fragment of
    Just "setemail" -> SetEmail
    Just "setpass" -> SetPassword
    _ -> JustMyAccount

