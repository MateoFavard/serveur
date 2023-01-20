module Page.SignIn exposing (Model, Msg, init, session, update, view)

import Browser exposing (Document)
import Browser.Navigation as Nav
import Html exposing (Html)
import Html.Attributes as Attr
import Html.Events as Events
import Html.Lazy as Lazy

import Session exposing (..)
import Route exposing (Route, SignUpFragment(..))
import Api
import Helpers exposing (..)


type alias Model = 
  { session : Session
  , userId : String
  , password : String
  , status : Status
  }


type Status
  = Initial
  | InProgress
  | Failed String


type Msg
  = UserId String
  | Password String
  | SignInRequest
  | SignInResponse (Api.Result (Maybe String))


init : Session -> ( Model, Cmd Msg )
init oldSession =
  (
    { session = oldSession
    , status = Initial
    , userId = ""
    , password = ""
    }
  , Cmd.none
  )


session : Model -> Session
session model = model.session


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
  case msg of
    UserId userId ->
      ( { model | userId = userId }, Cmd.none )

    Password password ->
      ( { model | password = password }, Cmd.none )

    SignInRequest ->
      ( { model | status = InProgress }
      , ( Api.signInRequest
          SignInResponse
          ( NotValidated { userId = model.userId, password = model.password } )
        )
      )

    SignInResponse response ->
      case response of
        Err Api.Internal ->
          ( { model | status = Failed "Erreur interne" }
          , Cmd.none
          )
        Err Api.Network ->
          ( { model | status = Failed "Impossible de se connecter au serveur" }
          , Cmd.none
          )
        Err Api.Unauthorized ->
          ( { model | status = Failed "Identifiant ou mot de passe invalide" }
          , Cmd.none
          )
        Err (Api.Message message) ->
          ( { model | status = Failed message }
          , Cmd.none
          )
        Ok maybeToken ->
          case maybeToken of
            Just token ->
              ( { model | session = validated model.session
                  { token = token
                  }
                }
              , Nav.pushUrl model.session.key "/myaccount"
              )
            Nothing ->
              ( { model | session = notValidated model.session
                  { userId = model.userId
                  , password = model.password
                  }
                }
              , Nav.pushUrl model.session.key "/validation"
              )



view : Model -> Html Msg
view model =
  Html.main_ [ ]
    [ Html.div [ Attr.class "box" ]
      [ Html.h1 [ ] [ Html.text "Connexion" ]
      , case model.status of
          Initial ->
            Html.p [ Attr.class "no-error" ] [ ]
          InProgress ->
            Html.p [ Attr.class "loading" ] [Html.text "Connexion" ]
          Failed msg ->
            Html.p [ Attr.class "error" ] [ Html.text msg ]
      , viewForm model
      ]
    ]


viewForm : Model -> Html Msg
viewForm model =
  Html.form [ Attr.id "signin", Events.onSubmit SignInRequest ]
    [ Lazy.lazy formInput
        { id = "userid"
        , type_ = "text"
        , label = "Identifiant"
        , required = True
        , onInput = UserId
        }
    , Lazy.lazy formInput
        { id = "password"
        , type_ = "password"
        , label = "Mot de passe"
        , required = True
        , onInput = Password
        }
    , Html.div [ Attr.class "input" ]
      [ Html.input [ Attr.type_ "submit", Attr.value "Se connecter" ] [ ]
      ]
    ]