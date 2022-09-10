import React from 'react'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux'
import { PersistGate } from 'redux-persist/integration/react'
import App from './components/app/App'
import { SetupCustomApi } from './http/CustomApi'
import { setupStore } from './store/store'

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement)
const { store, persistor } = setupStore()
export const customApi = SetupCustomApi(store)

root.render(
    <Provider store={store}>
        <PersistGate loading={null} persistor={persistor}>
            <App />
        </PersistGate>
    </Provider>
)
