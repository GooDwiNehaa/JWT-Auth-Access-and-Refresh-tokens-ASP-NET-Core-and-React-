import { AuthState } from '../store/reducers/AuthReducer'

export interface WithNetworkProps {
    authState: AuthState
    dispatch: any
}
