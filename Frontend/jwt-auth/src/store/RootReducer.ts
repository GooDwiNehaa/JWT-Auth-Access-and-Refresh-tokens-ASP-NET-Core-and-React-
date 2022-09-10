import { combineReducers } from '@reduxjs/toolkit'
import AuthReducer from './reducers/AuthReducer'
import UserReducer from './reducers/UserReducer'

const rootReducer = combineReducers({
    authReducer: AuthReducer,
    userReducer: UserReducer,
})

export default rootReducer
