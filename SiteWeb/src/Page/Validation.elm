module Page.Validation exposing (Model, Msg, init, session, update, view)

import Browser exposing (Document)
import Browser.Navigation as Nav
import Html exposing (Html)
import Html.Attributes as Attr
import Html.Lazy as Lazy
import Html.Events as Events

import Session exposing (..)
import Route exposing (Route, ValidationFragment(..))
import Helpers exposing (..)
import Api


type alias Model =
  { session : Session
  , password : String
  , passwordAgain : String
  , status : Status
  }


type Status
  = Initial
  | InProgress
  | Failed String


type Msg
  = Password String
  | PasswordAgain String
  | ValidationRequest
  | ValidationResponse (Api.Result ())


init : Session -> ValidationFragment -> ( Model, Cmd Msg )
init oldSession fragment =
  let
    model =
      { session = oldSession
      , password = ""
      , passwordAgain = ""
      , status = Initial
      }
  in 
    case oldSession.user of 
      NotValidated credentials ->
        ( model, Cmd.none )

      _ ->
        ( model, Nav.pushUrl model.session.key "/" )


session : Model -> Session
session model = model.session


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
  case msg of
    Password password ->
      ( { model | password = password }
      , Cmd.none
      )

    PasswordAgain passwordAgain ->
      ( { model
        | passwordAgain = passwordAgain
        , status = checkPasswords model.password passwordAgain 
        }
      , Cmd.none
      )

    ValidationRequest ->
      let
        status = checkPasswords model.password model.passwordAgain
      in
        if status == Initial then
          ( { model | status = InProgress }
          , ( Api.validationRequest
              ValidationResponse
              model.session.user
              { newPassword = model.password }
            )
          )
        else
          ( { model | status = status }, Cmd.none )

    ValidationResponse response ->
      case response of
        Err Api.Internal ->
          ( { model | status = Failed "Erreur interne" }
          , Cmd.none
          )
        Err Api.Network ->
          ( { model | status = Failed "Impossible de se connecter au serveur" }
          , Cmd.none
          )
        Err (Api.Message message) ->
          ( { model | status = Failed message }
          , Cmd.none
          )
        Err Api.Unauthorized ->
          ( { model | session = visitor model.session }
          , Nav.pushUrl model.session.key "/signin"
          )
        Ok _ ->
          ( { model | session = visitor model.session }
          , Nav.pushUrl model.session.key "/"
          )


checkPasswords : String -> String -> Status
checkPasswords password1 password2 =
  if password1 /= password2 then
    Failed "Les mots de passe ne correspondent pas"
  else if not (isStrong password1) then
    Failed (
      "Le mot de passe doit faire au moins 8 caractères et contenir "
      ++ "au moins une minuscule, une majuscule, un chiffre et un symbole"
    )
  else
    Initial


view : Model -> Html Msg
view model =
  Html.main_ [ ]
    [ Html.div [ Attr.class "box" ]
      [ Html.h1 [ ] [ Html.text "Validation du compte" ]
      , Html.p [ ] [ Html.text "Première connexion: veuillez changer votre mot de passe afin de valider votre compte." ]
      , case model.status of
          Initial ->
            Html.p [ Attr.class "no-error" ] [ ]
          InProgress ->
            Html.p [ Attr.class "loading" ] [Html.text "Validation" ]
          Failed msg ->
            Html.p [ Attr.class "error" ] [ Html.text msg ]
      , viewForm model
      ]
    ]


viewForm : Model -> Html Msg
viewForm model =
  Html.form [ Attr.id "validation", Events.onSubmit ValidationRequest ]
    [ Lazy.lazy formInput
        { id = "newPassword"
        , type_ = "password"
        , label = "Mot de passe"
        , required = True
        , onInput = Password
        }
    , Lazy.lazy formInput
        { id = "newPasswordAgain"
        , type_ = "password"
        , label = "Répéter le mot de passe"
        , required = True
        , onInput = PasswordAgain
        }
    , Html.div [ Attr.class "input" ]
      [ Html.input [ Attr.type_ "submit", Attr.value "Modifier le mot de passe" ] [ ]
      ]
    ]